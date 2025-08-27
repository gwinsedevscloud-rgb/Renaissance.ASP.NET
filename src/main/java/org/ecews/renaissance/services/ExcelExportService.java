package org.ecews.renaissance.services;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.RequiredArgsConstructor;

import java.util.List;

@Data
@AllArgsConstructor
@RequiredArgsConstructor
@NoArgsConstructor
public class ExcelExportService {
    private final List<String> headers = List.of(
        "S/N",
        "DATA COLLECTOR'S NAME",
        "PATIENT'S CODE",
        "GENDER",
        "AGE AS AT LAST BIRTHDAY (IN MONTHS FOR UNDER 60 Months AND YEARS  FOR OTHERS)",
        "SERVICES REQUIRED",
        "PHONE NUMBER",
        "ANCILLARY SERVICE REQUIRED",
        "DEWORMED?",
        "TOOTH BRUSH DISPENSED",
        "TOOTH PASTE  DISPENSED",
        "INSECTICIDE TREATED NET (ITN) DISPENSED",
        "PRESCRIPTION DISPENSED",
        "NUMBER OF PRESCRIPTION",
        "MARITAL STATUS",
        "OCCUPATION",
        "LEVEL OF EDUCATION",
        "SYSTOLIC",
        "DIASTOLIC",
        "BP RATE",
        "REFFERED FOR BP MONITORING?",
        "PULSE RATE",
        "BODY TEMPERATURE",
        "WEIGHT ( KG)",
        "HEIGHT ( M)",
        "BMI RANGE",
        "HIV RESULT",
        "WAS CONDOM DISPENSED?",
        "BLOOD SUGAR RESULT (mg/dl)",
        "RANDOM BLOOD SUGAR RANGE",
        "CHOLESTEROL RESULT (mg/dl)",
        "CHOLESTEROL RANGE",
        "MALARIA RESULT",
        "HEPATITIS B RESULT",
        "HEPATITIS C RESULT",
        "PSA RESULT",
        "DIAGNOSIS- GENERAL",
        "IF OTHERS, PLEASE SPECIFY",
        "TREATMENT OF MINOR AILMENTS/ OTHER MEDICAL SERVICES",
        "IF OTHERS, PLEASE SPECIFY",
        "REFERRED FOR FURTHER INVESTIGATIONS/ TREATMENT?",
        "EYE CARE SERVICE REQUIRED",
        "VISUAL ACUITY -  RIGHT EYE READING",
        "VISUAL ACUITY - LEFT EYE READING",
        "WAS GLASSES DISPENSED?",
        "DIAGNOSIS- OPTHAMOLOGIST",
        "EYE CARE TREATMENT- OPTHAMOLOGIST",
        "RECOMMENDED SURGERY",
        "IF OTHERS, PLEASE SPECIFY",
        "NAME OF PATIENTS",
        "CONTACT OF PATIENTS/ CAREGIVER",
        "EMAIL ADDRESS",
        "SURGERY PERFORMED",
        "IF OTHERS, PLEASE SPECIFY",
        "SURGERY SPECIFIC",
        "REFERRED FOR FURTHER INVESTIGTIONS/ TREATMENT?",
        "DIAGNOSIS- DENTAL",
        "IF OTHERS, PLEASE SPECIFY",
        "DENTAL TREATMENT",
        "IF OTHERS, PLEASE SPECIFY",
        "WHAT WAS DISPENSED?",
        "REFERRED FOR FURTHER INVESTIGTIONS/ TREATMENT?",
        "ANY COMMENTS//REMARK"
    );

}
