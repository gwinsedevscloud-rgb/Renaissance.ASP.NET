package org.ecews.renaissance.repositories;

import org.ecews.renaissance.domain.Patient;
import org.ecews.renaissance.repositories.projections.PatientReportProjection;
import org.ecews.renaissance.utils.ReportQueries;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;

import java.time.LocalDate;
import java.util.List;
import java.util.UUID;

public interface ReportRepository extends JpaRepository<Patient, UUID> {

    @Query(value = ReportQueries.LINE_LIST_QUERY, nativeQuery = true)
    List<PatientReportProjection> fetchReportRows(LocalDate startDate,
                                                  LocalDate endDate);
}
