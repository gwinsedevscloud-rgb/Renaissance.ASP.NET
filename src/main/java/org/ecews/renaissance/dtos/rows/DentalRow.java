package org.ecews.renaissance.dtos.rows;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDate;

@Data @NoArgsConstructor @AllArgsConstructor @Builder
public class DentalRow {
    Long sourceId;
    LocalDate serviceDate;
    String dentalDiagnoses, dentalOthersDiagnosis, dentalTreatments, dentalOthersTreatment, dentalDispensedItems, dentalServicesReferred, dentalReferred;
}
