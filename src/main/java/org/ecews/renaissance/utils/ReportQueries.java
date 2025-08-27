package org.ecews.renaissance.utils;

public class ReportQueries {

    public static final String LINE_LIST_QUERY = """
            WITH base AS (
                /* 1️⃣ PATIENT  (your missing table) */
                SELECT DISTINCT id AS patient_id
                FROM patient
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false

                UNION
                SELECT DISTINCT patient_id
                FROM ren_triage
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false

                UNION
                SELECT DISTINCT patient_id
                FROM consultation
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false

                UNION
                SELECT DISTINCT patient_id
                FROM ren_laboratory
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false

                UNION
                SELECT DISTINCT patient_id
                FROM ancillary_service
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false

                UNION
                SELECT DISTINCT patient_id
                FROM dental_consultation
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false

                UNION
                SELECT DISTINCT patient_id
                FROM ren_pharmacy_prescription
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false

                UNION
                SELECT DISTINCT patient_id
                FROM ren_ophthalmologist
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false
            ),

            latest_patient AS (
                SELECT DISTINCT ON (id)
                    id,
                    age,
                    age_unit,
                    sex,
                    phone_number,
                    marital_status,
                    tribe,
                    religion,
                    occupation,
                    address
                FROM patient
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false
                ORDER BY id, created_date DESC
            ),

            latest_triage AS (
                SELECT DISTINCT ON (patient_id)
                    patient_id,
                    weight,
                    height,
                    temperature,
                    systolic_bp,
                    diastolic_bp,
                    pulse_rate,
                    respiratory_rate,
                    ROUND(weight / POWER(height/100.0, 2), 2) AS bmi
                FROM ren_triage
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false
                  AND height IS NOT NULL AND height > 0
                  AND weight IS NOT NULL
                ORDER BY patient_id, created_date DESC
            ),

            latest_consult AS (
                SELECT DISTINCT ON (patient_id)
                    patient_id,
                    array_to_string(diagnoses, '; ')      AS diagnoses,
                    array_to_string(treatments, '; ')     AS treatments,
                    array_to_string(services_referred, '; ') AS services_referred,
                    CASE WHEN referred THEN 'Yes' ELSE 'No' END AS referred
                FROM consultation
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false
                ORDER BY patient_id, created_date DESC
            ),

            latest_lab AS (
                SELECT DISTINCT ON (patient_id)
                    patient_id,
                    MAX(CASE WHEN test_name = 'Random Blood Sugar'   THEN result END) AS random_blood_sugar,
                    MAX(CASE WHEN test_name = 'Malaria Parasite (RDT)' THEN result END) AS malaria_parasite_rdt,
                    MAX(CASE WHEN test_name = 'Hepatitis B'          THEN result END) AS hepatitis_b,
                    MAX(CASE WHEN test_name = 'Hepatitis C'          THEN result END) AS hepatitis_c,
                    MAX(CASE WHEN test_name = 'HIV Test'             THEN result END) AS hiv_test,
                    MAX(CASE WHEN test_name = 'Cholesterol'          THEN result END) AS cholesterol,
                    MAX(CASE WHEN test_name = 'PSA'                  THEN result END) AS psa
                FROM ren_laboratory
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false
                GROUP BY id, patient_id, created_date
                ORDER BY patient_id, created_date DESC
            ),

            latest_anc AS (
                SELECT DISTINCT ON (patient_id)
                    patient_id,
                    service AS ancillary_services,
                    CASE WHEN 'DEWORMING' = ANY(services) THEN 'Yes' END AS deworming,
                    CASE WHEN 'ITN' = ANY(services) THEN 'Yes' END AS itn,
                    CASE WHEN 'DENTAL MATERIAL - Tooth Paste' = ANY(services) THEN 'Yes' END AS dental_toothpaste,
                    CASE WHEN 'DENTAL MATERIAL - Tooth Brush' = ANY(services) THEN 'Yes' END AS dental_toothbrush
                FROM ancillary_service
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false
                ORDER BY patient_id, created_date DESC
            ),

            latest_dental AS (
                SELECT DISTINCT ON (patient_id)
                    patient_id,
                    array_to_string(diagnoses, '; ')         AS dental_diagnoses,
                    array_to_string(others_diagnosis, '; ')  AS dental_others_diagnosis,
                    array_to_string(treatments, '; ')        AS dental_treatments,
                    array_to_string(others_treatment, '; ')  AS dental_others_treatment,
                    array_to_string(dispensed_items, '; ')   AS dental_dispensed,
                    array_to_string(services_referred, '; ') AS dental_services_referred,
                    CASE WHEN referred THEN 'Yes' ELSE 'No' END AS dental_referred
                FROM dental_consultation
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false
                ORDER BY patient_id, created_date DESC
            ),

            latest_eye AS (
                SELECT DISTINCT ON (patient_id)
                    patient_id,
                    array_to_string(diagnosis, '; ')         AS eye_diagnosis,
                    array_to_string(others_diagnosis, '; ')  AS eye_others_diagnosis,
                    array_to_string(treatments, '; ')        AS eye_treatments,
                    array_to_string(others_treatment, '; ')  AS eye_others_treatment,
                    array_to_string(surgery, '; ')          AS eye_surgery,
                    array_to_string(other_surgery, '; ')    AS eye_other_surgery,
                    visual_acuity_left,
                    visual_acuity_right,
                    CASE WHEN cast(referred AS bool) THEN 'Yes' ELSE 'No' END AS eye_referred,
                    CASE WHEN glasses_dispensed = 'true' THEN 'Yes' ELSE 'No' END AS glasses_dispensed
                FROM ren_ophthalmologist
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false
                ORDER BY patient_id, created_date DESC
            ),

            rx_cnt AS (
                SELECT patient_id, COUNT(*) AS rx
                FROM ren_pharmacy_prescription
                WHERE CAST(created_date AS DATE) BETWEEN ?1 AND ?2
                  AND archived = false
                GROUP BY patient_id
            )

            SELECT
                p.id,
                p.age,
                p.age_unit,
                p.sex,
                p.phone_number,
                p.marital_status,
                p.tribe,
                p.religion,
                p.occupation,
                p.address,
                COALESCE(t.weight, 0)        AS weight,
                COALESCE(t.height, 0)        AS height,
                t.temperature,
                t.systolic_bp,
                t.diastolic_bp,
                t.pulse_rate,
                t.respiratory_rate,
                t.bmi,
                l.random_blood_sugar,
                l.malaria_parasite_rdt,
                l.hepatitis_b,
                l.hepatitis_c,
                l.hiv_test,
                l.cholesterol,
                l.psa,
                c.diagnoses               AS general_diagnosis,
                c.treatments,             AS general_treatments,
                c.services_referred       AS general_services_referred,
                c.referred                AS general_referred,
                d.dental_diagnoses,
                d.dental_others_diagnosis,
                d.dental_treatments,
                d.dental_others_treatment,
                d.dental_dispensed,
                d.dental_services_referred,
                d.dental_referred,
                e.eye_diagnosis,
                e.eye_others_diagnosis,
                e.eye_treatments,
                e.eye_others_treatment,
                e.eye_surgery,
                e.eye_other_surgery,
                e.visual_acuity_left,
                e.visual_acuity_right,
                e.eye_referred,
                e.glasses_dispensed,
                a.deworming,
                a.itn,
                a.dental_toothpaste,
                a.dental_toothbrush,
                COALESCE(rx.rx, 0)        AS prescription_count
            FROM base
            JOIN latest_patient     p ON p.id = base.patient_id
            LEFT JOIN latest_triage t ON t.patient_id = p.id
            LEFT JOIN latest_consult c ON c.patient_id = p.id
            LEFT JOIN latest_lab    l ON l.patient_id = p.id
            LEFT JOIN latest_anc    a ON a.patient_id = p.id
            LEFT JOIN latest_dental d ON d.patient_id = p.id
            LEFT JOIN latest_eye    e ON e.patient_id = p.id
            LEFT JOIN rx_cnt       rx ON rx.patient_id = p.id;
        """;

}
