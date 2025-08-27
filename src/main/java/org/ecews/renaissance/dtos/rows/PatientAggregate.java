package org.ecews.renaissance.dtos.rows;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class PatientAggregate {
    Long patientId;
    LocalDate serviceDatePatient;
    Integer patientAge; String patientAgeUnit; String patientSex;
    String patientPhoneNumber; String patientMaritalStatus; String patientTribe;
    String patientReligion; String patientOccupation; String patientAddress;
    @Builder.Default
    List<TriageRow> triage = new ArrayList<>();
    @Builder.Default List<ConsultationRow> consultations = new ArrayList<>();
    @Builder.Default List<LaboratoryRow> laboratories = new ArrayList<>();
    @Builder.Default List<AncillaryRow> ancillaryServices = new ArrayList<>();
    @Builder.Default List<DentalRow> dentalConsultations = new ArrayList<>();
    @Builder.Default List<PharmacyRow> pharmacy = new ArrayList<>();
    @Builder.Default List<EyeRow> eyeServices = new ArrayList<>();
    @Builder.Default
    Map<String, Object> computed = new LinkedHashMap<>();
}


