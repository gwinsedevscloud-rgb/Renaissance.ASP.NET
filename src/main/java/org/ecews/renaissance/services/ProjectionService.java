package org.ecews.renaissance.services;

import org.ecews.renaissance.dtos.rows.*;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Service;

import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.LocalDate;
import java.util.*;

@Service
class ProjectionService {
    private final JdbcTemplate jdbc;
    ProjectionService(JdbcTemplate jdbc) { this.jdbc = jdbc; }
    /** Build patientId -> PatientAggregate from PATIENT rows, then attach each module. */
    public List<PatientAggregate> fetchSince(LocalDate fromDate) {
        LinkedHashMap<Long, PatientAggregate> byPatient = fetchPatientsAsMap(fromDate);
        attachTriage(byPatient, fromDate);
        attachConsultations(byPatient, fromDate);
        attachLaboratories(byPatient, fromDate);
        attachAncillary(byPatient, fromDate);
        attachDental(byPatient, fromDate);
        attachPharmacy(byPatient, fromDate);
        attachEye(byPatient, fromDate);
        byPatient.values().forEach(this::computePatientDerivedFields);
        return new ArrayList<>(byPatient.values());
    }

    // ---------------------------
    // PATIENT base map
    // ---------------------------
    private LinkedHashMap<Long, PatientAggregate> fetchPatientsAsMap(LocalDate from) {
        final String sql = """
            SELECT
                p.id, p.age, p.age_unit, p.sex,
                p.phone_number, p.marital_status, p.tribe,
                p.religion, p.occupation, p.address,
                CAST(p.created_date AS DATE) as service_date_patient
            FROM patient p
            WHERE CAST(p.created_date AS DATE) >= ? and archived = false
        """;
        LinkedHashMap<Long, PatientAggregate> map = new LinkedHashMap<>();
        jdbc.query(sql, ps -> ps.setObject(1, from), rs -> {
            Long pid = rs.getLong("id");
            PatientAggregate agg = PatientAggregate.builder()
                .patientId(pid)
                .serviceDatePatient(rs.getObject("service_date_patient", LocalDate.class))
                .patientAge(getIntOrNull(rs, "age"))
                .patientAgeUnit(rs.getString("age_unit"))
                .patientSex(rs.getString("sex"))
                .patientPhoneNumber(rs.getString("phone_number"))
                .patientMaritalStatus(rs.getString("marital_status"))
                .patientTribe(rs.getString("tribe"))
                .patientReligion(rs.getString("religion"))
                .patientOccupation(rs.getString("occupation"))
                .patientAddress(rs.getString("address"))
                .build();
            map.put(pid, agg);
        });
        return map;
    }

    // ---------------------------
    // ATTACHERS (one per module)
    // ---------------------------
    private void attachTriage(Map<Long, PatientAggregate> map, LocalDate from) {
        final String sql = """
            SELECT id, patient_id, weight, height, temperature, systolic_bp, diastolic_bp, pulse_rate, respiratory_rate,
                   ROUND(weight / POWER(height / 100.0, 2), 2) AS bmi,
                   CAST(created_date AS DATE) as service_date_triage
            FROM ren_triage
            WHERE CAST(created_date AS DATE) >= ? and archived = false
              AND height IS NOT NULL AND height > 0 AND weight IS NOT NULL
        """;
        jdbc.query(sql, ps -> ps.setObject(1, from), (ResultSet rs) -> {
            Long pid = rs.getLong("patient_id");
            PatientAggregate agg = map.computeIfAbsent(pid, k -> PatientAggregate.builder().patientId(k).build());
            agg.getTriage().add(TriageRow.builder()
                .sourceId(rs.getLong("id"))
                .serviceDate(rs.getObject("service_date_triage", LocalDate.class))
                .weight(rs.getBigDecimal("weight"))
                .height(rs.getBigDecimal("height"))
                .bmi(rs.getBigDecimal("bmi"))
                .temperature(rs.getBigDecimal("temperature"))
                .systolicBp(getIntOrNull(rs, "systolic_bp"))
                .diastolicBp(getIntOrNull(rs, "diastolic_bp"))
                .pulseRate(getIntOrNull(rs, "pulse_rate"))
                .respiratoryRate(getIntOrNull(rs, "respiratory_rate"))
                .build());
        });
    }

    private void attachConsultations(Map<Long, PatientAggregate> map, LocalDate from) {
        final String sql = """
            SELECT id, patient_id,
                   array_to_string(diagnoses, '; ') AS diagnoses,
                   array_to_string(treatments, '; ') AS treatments,
                   array_to_string(services_referred, '; ') AS services_referred,
                   CASE WHEN referred THEN 'Yes' ELSE 'No' END AS referred,
                   CAST(created_date AS DATE) as service_date_consultation
            FROM consultation
            WHERE CAST(created_date AS DATE) >= ? and archived = false
        """;
        jdbc.query(sql, ps -> ps.setObject(1, from), (ResultSet rs) -> {
            Long pid = rs.getLong("patient_id");
            PatientAggregate agg = map.computeIfAbsent(pid, k -> PatientAggregate.builder().patientId(k).build());
            agg.getConsultations().add(ConsultationRow.builder()
                .sourceId(rs.getLong("id"))
                .serviceDate(rs.getObject("service_date_consultation", LocalDate.class))
                .consultationDiagnoses(rs.getString("diagnoses"))
                .consultationTreatments(rs.getString("treatments"))
                .consultationServicesReferred(rs.getString("services_referred"))
                .consultationReferred(rs.getString("referred"))
                .build());
        });
    }

    private void attachLaboratories(Map<Long, PatientAggregate> map, LocalDate from) {
        final String sql = """
            SELECT id, patient_id, CAST(created_date AS DATE) as service_date_laboratory,
                   MAX(CASE WHEN test_name = 'Random Blood Sugar' THEN result END) AS random_blood_sugar,
                   MAX(CASE WHEN test_name = 'Malaria Parasite (RDT)' THEN result END) AS malaria_parasite_rdt,
                   MAX(CASE WHEN test_name = 'Hepatitis B' THEN result END) AS hepatitis_b,
                   MAX(CASE WHEN test_name = 'Hepatitis C' THEN result END) AS hepatitis_c,
                   MAX(CASE WHEN test_name = 'HIV Test' THEN result END) AS hiv_test,
                   MAX(CASE WHEN test_name = 'Cholesterol' THEN result END) AS cholesterol,
                   MAX(CASE WHEN test_name = 'PSA' THEN result END) AS psa
            FROM ren_laboratory
            WHERE CAST(created_date AS DATE) >= ? AND archived = false
            GROUP BY id, patient_id, CAST(created_date AS DATE)
        """;
        jdbc.query(sql, ps -> ps.setObject(1, from), (ResultSet rs) -> {
            Long pid = rs.getLong("patient_id");
            PatientAggregate agg = map.computeIfAbsent(pid, k -> PatientAggregate.builder().patientId(k).build());
            agg.getLaboratories().add(LaboratoryRow.builder()
                .sourceId(rs.getLong("id"))
                .serviceDate(rs.getObject("service_date_laboratory", LocalDate.class))
                .randomBloodSugar(rs.getString("random_blood_sugar"))
                .malariaParasiteRdt(rs.getString("malaria_parasite_rdt"))
                .hepatitisB(rs.getString("hepatitis_b"))
                .hepatitisC(rs.getString("hepatitis_c"))
                .hivTest(rs.getString("hiv_test"))
                .cholesterol(rs.getString("cholesterol"))
                .psa(rs.getString("psa"))
                .build());
        });
    }

    private void attachAncillary(Map<Long, PatientAggregate> map, LocalDate from) {
        final String sql = """
            SELECT id, patient_id, CAST(created_date AS DATE) as service_date_ancillary,
                   CASE WHEN 'DEWORMING' = ANY(services) THEN 'Yes' END AS deworming,
                   CASE WHEN 'ITN' = ANY(services) THEN 'Yes' END AS itn,
                   CASE WHEN 'DENTAL MATERIAL - Tooth Paste' = ANY(services) THEN 'Yes' END AS dental_toothpaste,
                   CASE WHEN 'DENTAL MATERIAL - Tooth Brush' = ANY(services) THEN 'Yes' END AS dental_toothbrush
            FROM ancillary_service
            WHERE CAST(created_date AS DATE) >= ? AND archived = false
        """;
        jdbc.query(sql, ps -> ps.setObject(1, from), (ResultSet rs) -> {
            Long pid = rs.getLong("patient_id");
            PatientAggregate agg = map.computeIfAbsent(pid, k -> PatientAggregate.builder().patientId(k).build());
            agg.getAncillaryServices().add(AncillaryRow.builder()
                .sourceId(rs.getLong("id"))
                .serviceDate(rs.getObject("service_date_ancillary", LocalDate.class))
                .ancillaryDeworming(rs.getString("deworming"))
                .ancillaryItn(rs.getString("itn"))
                .ancillaryDentalToothpaste(rs.getString("dental_toothpaste"))
                .ancillaryDentalToothbrush(rs.getString("dental_toothbrush"))
                .build());
        });
    }

    private void attachDental(Map<Long, PatientAggregate> map, LocalDate from) {
        final String sql = """
            SELECT id, patient_id, CAST(created_date AS DATE) as service_date_dental_consultation,
                   array_to_string(diagnoses, '; ') AS diagnoses,
                   array_to_string(others_diagnosis, '; ') AS others_diagnosis,
                   array_to_string(treatments, '; ') AS treatments,
                   array_to_string(others_treatment, '; ') AS others_treatment,
                   array_to_string(dispensed_items, '; ') AS dispensed_items,
                   array_to_string(services_referred, '; ') AS services_referred,
                   CASE WHEN referred THEN 'Yes' ELSE 'No' END AS referred
            FROM dental_consultation
            WHERE CAST(created_date AS DATE) >= ? AND archived = false
        """;
        jdbc.query(sql, ps -> ps.setObject(1, from), (ResultSet rs) -> {
            Long pid = rs.getLong("patient_id");
            PatientAggregate agg = map.computeIfAbsent(pid, k -> PatientAggregate.builder().patientId(k).build());
            agg.getDentalConsultations().add(DentalRow.builder()
                .sourceId(rs.getLong("id"))
                .serviceDate(rs.getObject("service_date_dental_consultation", LocalDate.class))
                .dentalDiagnoses(rs.getString("diagnoses"))
                .dentalOthersDiagnosis(rs.getString("others_diagnosis"))
                .dentalTreatments(rs.getString("treatments"))
                .dentalOthersTreatment(rs.getString("others_treatment"))
                .dentalDispensedItems(rs.getString("dispensed_items"))
                .dentalServicesReferred(rs.getString("services_referred"))
                .dentalReferred(rs.getString("referred"))
                .build());
        });
    }

    private void attachPharmacy(Map<Long, PatientAggregate> map, LocalDate from) {
        final String sql = """
            select id, patient_id, drug_category, drug_name, status,
                   CAST(created_date AS DATE) as service_date_pharmacy
            from ren_pharmacy_prescription where CAST(created_date AS DATE) >= ? AND archived = false
        """;
        jdbc.query(sql, ps -> ps.setObject(1, from), (ResultSet rs) -> {
            Long pid = rs.getLong("patient_id");
            PatientAggregate agg = map.computeIfAbsent(pid, k -> PatientAggregate.builder().patientId(k).build());
            agg.getPharmacy().add(PharmacyRow.builder()
                .sourceId(rs.getLong("id"))
                .serviceDate(rs.getObject("service_date_pharmacy", LocalDate.class))
                .pharmacyDrugCategory(rs.getString("drug_category"))
                .pharmacyDrugName(rs.getString("drug_name"))
                .pharmacyStatus(rs.getString("status"))
                .build());
        });
    }

    private void attachEye(Map<Long, PatientAggregate> map, LocalDate from) {
        final String sql = """
            select id, patient_id, CAST(created_date AS DATE) as service_date_eye,
                   array_to_string(diagnosis, '; ') AS diagnoses,
                   array_to_string(others_diagnosis, '; ') AS others_diagnosis,
                   array_to_string(treatments, '; ') AS treatments,
                   array_to_string(others_treatment, '; ') AS others_treatment,
                   array_to_string(surgery, '; ') AS surgery,
                   array_to_string(other_surgery, '; ') AS other_surgery,
                   visual_acuity_left, visual_acuity_right,
                   CASE WHEN cast(referred as bool) THEN 'Yes' ELSE 'No' END AS referred,
                   CASE WHEN glasses_dispensed = 'true' THEN 'Yes' ELSE 'No' END AS glasses_dispensed
            from ren_ophthalmologist where CAST(created_date AS DATE) >= ? AND archived = false
        """;
        jdbc.query(sql, ps -> ps.setObject(1, from), (ResultSet rs) -> {
            Long pid = rs.getLong("patient_id");
            PatientAggregate agg = map.computeIfAbsent(pid, k -> PatientAggregate.builder().patientId(k).build());
            agg.getEyeServices().add(EyeRow.builder()
                .sourceId(rs.getLong("id"))
                .serviceDate(rs.getObject("service_date_eye", LocalDate.class))
                .eyeDiagnoses(rs.getString("diagnoses"))
                .eyeOthersDiagnosis(rs.getString("others_diagnosis"))
                .eyeTreatments(rs.getString("treatments"))
                .eyeOthersTreatment(rs.getString("others_treatment"))
                .eyeSurgery(rs.getString("surgery"))
                .eyeOtherSurgery(rs.getString("other_surgery"))
                .visualAcuityLeft(rs.getString("visual_acuity_left"))
                .visualAcuityRight(rs.getString("visual_acuity_right"))
                .eyeReferred(rs.getString("referred"))
                .eyeGlassesDispensed(rs.getString("glasses_dispensed"))
                .build());
        });
    }

    // ---------------------------
    // Derived fields at patient-level (optional)
    // ---------------------------
    private void computePatientDerivedFields(PatientAggregate p) {
        // Example: last BMI
        p.getTriage().stream()
            .filter(t -> t.getBmi() != null)
            .max(Comparator.comparing(TriageRow::getServiceDate, Comparator.nullsLast(Comparator.naturalOrder())))
            .ifPresent(last -> p.getComputed().put("last_bmi", last.getBmi()));

        // Example: ever hypertensive within range
        boolean everHighBp = p.getTriage().stream().anyMatch(t ->
            (t.getSystolicBp() != null && t.getSystolicBp() >= 140) ||
                (t.getDiastolicBp() != null && t.getDiastolicBp() >= 90)
        );
        p.getComputed().put("ever_hypertensive", everHighBp ? "Yes" : "No");
    }

    private Integer getIntOrNull(ResultSet rs, String col) throws SQLException {
        int v = rs.getInt(col); return rs.wasNull() ? null : v;
    }
}
