package org.ecews.renaissance.dtos.rows;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.time.LocalDate;

@Data @NoArgsConstructor @AllArgsConstructor @Builder
public class EyeRow {
    Long sourceId;
    LocalDate serviceDate;
    String eyeDiagnoses, eyeOthersDiagnosis, eyeTreatments, eyeOthersTreatment, eyeSurgery, eyeOtherSurgery, visualAcuityLeft, visualAcuityRight, eyeReferred, eyeGlassesDispensed;
}
