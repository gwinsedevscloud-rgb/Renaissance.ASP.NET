import {  http, HttpResponse } from 'msw';

export const handlers = [
    http.get('/api/ren/patients', () => {
        console.log("Working...")
        return HttpResponse.json([
            {
                "id": 1,
                "clientNumber": "AKT001",
                "surname": "Okeke",
                "firstName": "Chinedu",
                "age": 30,
                "ageUnit": "years",
                "sex": "male",
                "tribe": "Igbo",
                "religion": "Christian",
                "address": "123 Market Road, Akuku-Toru",
                "occupation": "Teacher",
                "education": "Tertiary",
                "maritalStatus": "Married"
            },
            {
                "id": 2,
                "clientNumber": "AKT002",
                "surname": "Aminu",
                "firstName": "Fatima",
                "age": 24,
                "ageUnit": "years",
                "sex": "female",
                "tribe": "Hausa",
                "religion": "Islam",
                "address": "45 River Street, Akuku-Toru",
                "occupation": "Nurse",
                "education": "Tertiary",
                "maritalStatus": "Single"
            },
            {
                "id": 3,
                "clientNumber": "AKT003",
                "surname": "Ebiowei",
                "firstName": "Tamuno",
                "age": 6,
                "ageUnit": "months",
                "sex": "male",
                "tribe": "Ijaw",
                "religion": "Christian",
                "address": "12 Coastal Lane, Akuku-Toru",
                "occupation": "Unemployed",
                "education": "None",
                "maritalStatus": "Single"
            },
            {
                "id": 4,
                "clientNumber": "AKT004",
                "surname": "Okafor",
                "firstName": "Ngozi",
                "age": 45,
                "ageUnit": "years",
                "sex": "female",
                "tribe": "Igbo",
                "religion": "Christian",
                "address": "78 Main Street, Akuku-Toru",
                "occupation": "Trader",
                "education": "Secondary",
                "maritalStatus": "Divorced"
            },
            {
                "id": 5,
                "clientNumber": "AKT005",
                "surname": "Sambo",
                "firstName": "Ibrahim",
                "age": 60,
                "ageUnit": "years",
                "sex": "male",
                "tribe": "Hausa",
                "religion": "Islam",
                "address": "19 Palm Avenue, Akuku-Toru",
                "occupation": "Farmer",
                "education": "Primary",
                "maritalStatus": "Married"
            }
        ])
    }),
];
