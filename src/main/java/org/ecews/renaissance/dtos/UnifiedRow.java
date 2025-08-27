package org.ecews.renaissance.dtos;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.LinkedHashMap;
import java.util.Map;

@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
class UnifiedRow {
    Long sourceId;           // original table id
    String moduleType;       // PATIENT, TRIAGE, CONSULTATION, LABORATORY, ANCILLARY, DENTAL_CONSULTATION, PHARMACY, EYE_SERVICE

    Long patientId;          // for PATIENT rows this is p.id
    LocalDate serviceDate;   // module-specific service date

    // ================= PATIENT (prefixed) =================
    Integer patientAge;
    String patientAgeUnit;
    String patientSex;
    String patientPhoneNumber;
    String patientMaritalStatus;
    String patientTribe;
    String patientReligion;
    String patientOccupation; String patientAddress;

    // ================= TRIAGE =================
    BigDecimal weight;
    BigDecimal height;
    BigDecimal bmi;
    BigDecimal temperature;
    Integer systolicBp;
    Integer diastolicBp;
    Integer pulseRate;
    Integer respiratoryRate;

    // ================= CONSULTATION (prefixed) =================
    String consultationDiagnoses;
    String consultationTreatments;
    String consultationServicesReferred;
    String consultationReferred;

    // ================= DENTAL (prefixed) =================
    String dentalDiagnoses;
    String dentalOthersDiagnosis;
    String dentalTreatments;
    String dentalOthersTreatment;
    String dentalServicesReferred;
    String dentalReferred;
    String dentalDispensedItems;

    // ================= EYE (prefixed) =================
    String eyeDiagnoses;
    String eyeOthersDiagnosis;
    String eyeTreatments;
    String eyeOthersTreatment;
    String eyeSurgery;
    String eyeOtherSurgery;
    String eyeVisualAcuityLeft;
    String eyeVisualAcuityRight;
    String eyeReferred;
    String eyeGlassesDispensed;

    // ================= LABORATORY (prefixed) =================
    String labRandomBloodSugar;
    String labMalariaParasiteRdt;
    String labHepatitisB;
    String labHepatitisC;
    String labHivTest;
    String labCholesterol;
    String labPsa;

    // ================= ANCILLARY (prefixed) =================
    String ancillaryDeworming;
    String ancillaryItn;
    String ancillaryDentalToothpaste;
    String ancillaryDentalToothbrush;

    // ================= PHARMACY (prefixed) =================
    String pharmacyDrugCategory;
    String pharmacyDrugName;
    String pharmacyStatus;

    Map<String, Object> computed = new LinkedHashMap<>();
}
