package org.ecews.renaissance.services;

import lombok.RequiredArgsConstructor;
import org.ecews.renaissance.repositories.ReportRepository;
import org.ecews.renaissance.repositories.mappers.PatientReportRow;
import org.ecews.renaissance.repositories.projections.PatientReportProjection;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDate;
import java.util.Arrays;
import java.util.List;
import java.util.Locale;
import java.util.Objects;
import java.util.concurrent.atomic.AtomicLong;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class ReportService {

    private final ReportRepository repository;

    public List<PatientReportRow> buildRows(LocalDate startDate, LocalDate endDate) {
        List<PatientReportProjection> projs = repository.fetchReportRows(startDate, endDate);
        AtomicLong counter = new AtomicLong(1);
        return projs.stream().map(p -> map(p, counter.getAndIncrement())).toList();
    }

    private PatientReportRow map(PatientReportProjection p, long sn) {
        var dewormingDone = Objects.equals(p.deworming(), "Yes") ? Boolean.TRUE : Boolean.FALSE;
        var itnDone = Objects.equals(p.itn(), "Yes") ? Boolean.TRUE : Boolean.FALSE;
        var dentalToothpaste = Objects.equals(p.dentalToothpaste(), "Yes") ? Boolean.TRUE : Boolean.FALSE;
        var dentalToothbrush = Objects.equals(p.dentalToothbrush(), "Yes") ? Boolean.TRUE : Boolean.FALSE;
        var age = p.age();
        var ageUnit = p.ageUnit();
        var pregnanc

        return PatientReportRow.builder()
             .sn(sn)
             .dataCollectorName("System")  // or fetch from logged-in user
             .patientCode("P" + p.id())
             .gender(p.sex())
             .ageAtLastBirthday(formatAge(p.age(), p.ageUnit()))
             .phoneNumber(p.phoneNumber())
             .maritalStatus(p.maritalStatus())
             .occupation(p.occupation())
             .weightKg(p.weight())
             .heightM(p.height().divide(BigDecimal.valueOf(100), 2, RoundingMode.HALF_UP))
             .bmiRange(bmiRange(p.bmi()))
             .systolic(p.systolicBp())
             .diastolic(p.diastolicBp())
             .bpRate(bpRate(p.systolicBp(), p.diastolicBp()))
             .pulseRate(p.pulseRate())
             .bodyTemperature(p.temperature())
             .hivResult(emptyToBlank(p.hivTest()))
             .bloodSugarMgDl(p.getRandomBloodSugar())
             .bloodSugarRange(randomSugarRange(p.randomBloodSugar()))
             .cholesterolMgDl(p.cholesterol())
             .cholesterolRange(cholesterolRange(p.cholesterol()))
             .malariaResult(emptyToBlank(p.malariaParasiteRdt()))
             .hepatitisBResult(emptyToBlank(p.hepatitisB()))
             .hepatitisCResult(emptyToBlank(p.hepatitisC()))
             .psaResult(emptyToBlank(p.psa()))
             .diagnosisGeneral(p.generalDiagnosis())
             .treatmentMinorAilments(p.generalTreatments())
             .referredFurtherInvestigation(p.generalReferred())
             .eyeCareServiceRequired(p.getEyeDiagnosis() != null ? "Yes" : "No")
             .visualAcuityLeft(p.visualAcuityLeft())
             .visualAcuityRight(p.visualAcuityRight())
             .glassesDispensed(emptyToBlank(p.glassesDispensed()))
             .diagnosisOphthalmologist(emptyToBlank(p.eyeDiagnosis()))
             .eyeCareTreatment(emptyToBlank(p.eyeTreatments()))
             .recommendedSurgery(emptyToBlank(p.eyeSurgery()))
             .patientName(null) // not in DB
             .contactOfPatientOrCaregiver(p.phoneNumber())
             .diagnosisDental(emptyToBlank(p.dentalDiagnoses()))
             .dentalTreatment(emptyToBlank(p.dentalTreatments()))
             .whatWasDispensed(emptyToBlank(p.dentalDispensed()))
             .ancillaryService(p.ancillaryServices())
             .dewormed(categoriseAncillaryService(age, ageUnit, Boolean.TRUE, p.sex(), "DEWORMING", )
             .toothBrushDispensed(containsService(p.getAncillaryServices(), "DENTAL MATERIAL - Tooth Brush"))
             .toothPasteDispensed(containsService(p.getAncillaryServices(), "DENTAL MATERIAL - Tooth Paste"))
             .itnDispensed(containsService(p.getAncillaryServices(), "ITN"))
             .numberOfPrescriptions(p.getPrescriptionCount())
             .build();
    }

    private String categoriseAncillaryService(Integer age,
                                              String ageUnit,
                                              Boolean isPregnant,
                                              String sex,
                                              String service,
                                              boolean rendered) {

        if (!rendered) {
            return "NA";
        }

        // Defensive null handling
        Objects.requireNonNull(age, "age");
        Objects.requireNonNull(ageUnit, "ageUnit");
        Objects.requireNonNull(service, "service");

        // Normalise inputs once
        String unit = ageUnit.trim().toUpperCase(Locale.ROOT);
        String svc  = service.trim().toUpperCase(Locale.ROOT);
        boolean female = "FEMALE".equalsIgnoreCase(sex);

        // Convert months to years for uniform checks
        int ageYears = unit.startsWith("MONTH") ? age / 12 : age;

        // Deworming & dental-material rules
        if (svc.contains("DEWORMING") || svc.contains("DENTAL MATERIAL")) {
            if (ageYears < 5) {
                return "UNDER 5";
            }
            if (ageYears <= 12) {
                return "5 - 12 YEARS OLD";
            }
            if (ageYears <= 18) {
                return "TEENAGER";
            }
            if (female && ageYears <= 50) {
                return "REPRODUCTIVE AGE WOMEN";
            }
            if (ageYears <= 69) {
                return "ELDERLY";
            }
            return "OTHERS";
        }

        // Default rules for other services
        if (ageYears < 5) {
            return "UNDER 5";
        }
        if (Boolean.TRUE.equals(isPregnant)) {
            return "PREGNANT WOMEN";
        }
        if (ageYears <= 69) {
            return "ELDERLY";
        }
        return "OTHERS";
    }

    /* ---------- helper functions ---------- */
    private String formatAge(Integer age, String unit) {
        if (age == null) return "";
        if ("MONTH".equalsIgnoreCase(unit) && age < 60) return age + "M";
        return age + "Y";
    }
    private String bmiRange(BigDecimal bmi) {
        if (bmi == null) return "";
        if (bmi.compareTo(BigDecimal.valueOf(18.5)) < 0) return "Underweight";
        if (bmi.compareTo(BigDecimal.valueOf(25)) < 0) return "Normal";
        if (bmi.compareTo(BigDecimal.valueOf(30)) < 0) return "Overweight";
        return "Obese";
    }
    private String bpRate(Integer sys, Integer dia) {
        if (sys == null || dia == null) return "";
        if (sys < 90 || dia < 60) return "Low";
        if (sys <= 120 && dia <= 80) return "Normal";
        if (sys <= 130 && dia <= 85) return "Elevated";
        if (sys <= 140 && dia <= 90) return "Stage 1";
        return "Stage 2";
    }
    private String randomSugarRange(BigDecimal val) {
        if (val == null) return "";
        if (val.compareTo(BigDecimal.valueOf(140)) <= 0) return "Normal";
        if (val.compareTo(BigDecimal.valueOf(200)) <= 0) return "Pre-diabetes";
        return "High";
    }
    private String cholesterolRange(BigDecimal val) {
        if (val == null) return "";
        if (val.compareTo(BigDecimal.valueOf(200)) <= 0) return "Desirable";
        if (val.compareTo(BigDecimal.valueOf(240)) <= 0) return "Borderline";
        return "High";
    }
    private String join(String... arr) {
        return Arrays.stream(arr).filter(Objects::nonNull).collect(Collectors.joining("; "));
    }
    private String anyYes(String... arr) {
        return Arrays.stream(arr).anyMatch("Yes"::equals) ? "Yes" : "No";
    }
    private String containsService(String[] services, String keyword) {
        if (services == null) return "No";
        return Arrays.stream(services).anyMatch(keyword::equals) ? "Yes" : "No";
    }
    private String emptyToBlank(String s) {
        return s == null ? "" : s;
    }
}
