package org.ecews.renaissance.dtos.rows;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.math.BigDecimal;
import java.time.LocalDate;

@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class TriageRow {
    Long sourceId;
    LocalDate serviceDate;
    BigDecimal weight, height, bmi, temperature;
    Integer systolicBp, diastolicBp, pulseRate, respiratoryRate;
}
