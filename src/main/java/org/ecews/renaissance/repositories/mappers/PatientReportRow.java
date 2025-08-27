package org.ecews.renaissance.repositories.mappers;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.math.BigDecimal;

@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class PatientReportRow {
    private Long sn;
    private String dataCollectorName;         // fixed or taken from user
    private String patientCode;
    private String gender;
    private String ageAtLastBirthday;
    private String servicesRequired;
    private String phoneNumber;
    private String ancillaryServiceRequired;
    private String dewormed;
    private String ancillaryService;
    private String toothBrushDispensed;
    private String toothPasteDispensed;
    private String itnDispensed;
    private String prescriptionDispensed;
    private Integer numberOfPrescriptions;
    private String maritalStatus;
    private String occupation;
    private String levelOfEducation;          // not in DB → leave blank
    private Integer systolic;
    private Integer diastolic;
    private String bpRate;                    // computed
    private String referredForBpMonitoring;
    private Integer pulseRate;
    private BigDecimal bodyTemperature;
    private BigDecimal weightKg;
    private BigDecimal heightM;
    private String bmiRange;                  // computed
    private String hivResult;
    private String condomDispensed;           // always blank (no source)
    private BigDecimal bloodSugarMgDl;
    private String bloodSugarRange;           // computed
    private BigDecimal cholesterolMgDl;
    private String cholesterolRange;          // computed
    private String malariaResult;
    private String hepatitisBResult;
    private String hepatitisCResult;
    private String psaResult;
    private String diagnosisGeneral;
    private String diagnosisGeneralOther;
    private String treatmentMinorAilments;
    private String treatmentMinorAilmentsOther;
    private String referredFurtherInvestigation;
    private String eyeCareServiceRequired;
    private String visualAcuityRight;
    private String visualAcuityLeft;
    private String glassesDispensed;
    private String diagnosisOphthalmologist;
    private String eyeCareTreatment;
    private String recommendedSurgery;
    private String recommendedSurgeryOther;
    private String patientName;
    private String contactOfPatientOrCaregiver;
    private String emailAddress;
    private String surgeryPerformed;
    private String surgeryPerformedOther;
    private String surgerySpecific;
    private String referredFurtherInvestigationEye;
    private String diagnosisDental;
    private String diagnosisDentalOther;
    private String dentalTreatment;
    private String dentalTreatmentOther;
    private String whatWasDispensed;
    private String referredFurtherInvestigationDental;
    private String commentsRemarks;
}
