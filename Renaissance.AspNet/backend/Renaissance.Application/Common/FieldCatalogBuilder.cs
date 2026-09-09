using Renaissance.Application.DTOs;

namespace Renaissance.Application.Common;

/// <summary>
/// Static clinical option lists shipped to Field tablets on pull (mirrors web catalogs).
/// </summary>
public static class FieldCatalogBuilder
{
    public static FieldCatalogDto Build()
    {
        var catalog = new FieldCatalogDto
        {
            LabTests =
            [
                new() { Name = "Fasting Blood Sugar", StandardResults = [], DefaultNote = null },
                new() { Name = "Random Blood Sugar", StandardResults = [], DefaultNote = null },
                new() { Name = "Malaria Parasite_RDT", StandardResults = [], DefaultNote = null },
                new() { Name = "Hepatitis B", StandardResults = [], DefaultNote = null },
                new() { Name = "Hepatitis C", StandardResults = [], DefaultNote = null },
                new() { Name = "HIV Test", StandardResults = [], DefaultNote = null },
                new() { Name = "Cholesterol", StandardResults = [], DefaultNote = null },
                new() { Name = "PSA", StandardResults = [], DefaultNote = null }
            ],
            DrugCategories =
            [
                "Antimalarial", "Antihypertensive", "Antidiabetic", "Analgesic", "Antibiotic",
                "Supplement", "Antiretroviral", "Antitubercular", "Antenatal", "Antiglaucoma"
            ],
            ConsultationDiagnoses =
            [
                "Uncomplicated malaria", "Essential hypertension", "Type 2 diabetes mellitus",
                "Upper respiratory tract infection", "Antenatal booking — early pregnancy",
                "Typhoid fever", "Pneumonia", "Urinary tract infection", "Peptic ulcer disease",
                "Sickle cell vaso-occlusive crisis"
            ],
            ConsultationTreatments =
            [
                "Artemether/Lumefantrine", "Amlodipine 5 mg daily", "Metformin 500 mg twice daily",
                "Paracetamol and rest", "Folic acid and tetanus toxoid", "Ceftriaxone IM",
                "Amoxicillin/clavulanate", "Low-salt diet counselling", "Oral rehydration therapy",
                "Pain control and hydration"
            ],
            ReferralServices = ["Laboratory", "Pharmacy", "Dental", "Ancillary", "Optometry", "Ophthalmology", "Triage"],
            DentalDiagnoses = ["Routine Check Up", "Preventive Care", "Restorative Services", "Oral Surgery", "Others"],
            DentalOtherDiagnoses =
            [
                "ACUTE PERIODONTITIS",
                "Acute pericoronitis",
                "BLEEDING GUM",
                "CHRONIC PERIODONTITIS",
                "Clinically healthy mouth",
                "Collapsed filling",
                "Cracked tooth syndrome",
                "DENTAL CARIES",
                "DENTAL PLAQUE",
                "Dentoalveolar abscess on lower right 1st premolar",
                "Denture",
                "Enamel hypoplasi",
                "Erupting incisors",
                "Erupting molars",
                "Erupting upper left 3",
                "FETOR ORIS",
                "Fractured tooth",
                "GINGIVAL POCKET",
                "GINGIVAL RECESSION",
                "Missing tooth",
                "Mobile tooth",
                "Pericoronitis 2° imparted third molar",
                "Periodontal abscess",
                "Recurrent Pericoronitis left last molar",
                "Retain destinies tooth; Lingual emptying incisor",
                "Stains",
                "Supernumerary teeth",
                "TOOTH SENSITIVITY",
                "Toothwear lesion",
                "Others"
            ],
            DentalTreatments =
            [
                "Comprehensive oral examination",
                "Professional cleaning",
                "Scaling and Polishing",
                "Fluoride treatments",
                "Sealants",
                "Oral hygiene education",
                "Fillings",
                "Extractions",
                "Giving of toothbrushes"
            ],
            DentalServiceGroups =
            [
                new() { Category = "Routine Check Up", Services = ["Comprehensive oral examination", "Professional cleaning"] },
                new() { Category = "Preventive Care", Services = ["Scaling and Polishing", "Fluoride treatments", "Sealants", "Oral hygiene education"] },
                new() { Category = "Restorative Services", Services = ["Fillings"] },
                new() { Category = "Oral Surgery", Services = ["Extractions"] },
                new() { Category = "Others", Services = ["Giving of toothbrushes"] }
            ],
            DentalDispensedItems =
            [
                "Toothbrush", "Dental filling material", "Fluoride varnish", "Sealant material",
                "Chlorhexidine mouthwash", "Ibuprofen 400 mg", "Amoxicillin 500 mg"
            ],
            VisualAcuityOptions = ["6/6", "6/9", "6/12", "6/18", "6/24", "6/36", "6/60", "3/60", "CF", "HM", "LP", "NLP"],
            EyeDiagnoses =
            [
                "ALLERGIC CONJUNCTIVITIS",
                "ASTIGMATISM",
                "BACTERIAL CONJUNCTIVITIS",
                "CORNEA OPACITY",
                "DRY EYE",
                "FOREIGN BODY",
                "GLAUCOMA",
                "HYPEROPIA",
                "IMMATURE CATARACT",
                "MATURE CATARACT",
                "MYOPIA",
                "PRESBYOPIA",
                "PTERYGIUM",
                "Vernal conjunctivitis"
            ],
            EyeTreatments = ["MEDICATION", "REFRACTION", "SURGERY"],
            EyeServices =
            [
                "Eye Health Education", "Visual Acuity testing", "Ophthalmoscopy",
                "Refraction and dispensing of reading glasses", "Glaucoma screening and management",
                "Cataract screening and referral", "Management of minor ocular conditions"
            ],
            EyeMedications =
            [
                "Chloramphenicol eye drop",
                "Antallerg eye drop",
                "Timolol eye drop",
                "Hypromellose eyedrop",
                "Bet-N eyedrop",
                "Ivyflur eye drop"
            ]
        };

        void AddDrugs(string category, (string Name, string Dosage, string Frequency, string Duration)[] drugs)
        {
            foreach (var d in drugs)
            {
                catalog.Drugs.Add(new FieldDrugOptionDto
                {
                    Category = category,
                    Name = d.Name,
                    Dosage = d.Dosage,
                    Frequency = d.Frequency,
                    Duration = d.Duration
                });
            }
        }

        AddDrugs("Antimalarial",
        [
            ("Artemether/Lumefantrine (Coartem)", "80/480 mg", "Twice daily", "3 days"),
            ("Artesunate-Amodiaquine", "100/270 mg", "Once daily", "3 days"),
            ("Quinine Sulphate", "600 mg", "Three times daily", "7 days")
        ]);
        AddDrugs("Antihypertensive",
        [
            ("Amlodipine", "5 mg", "Once daily", "30 days"),
            ("Lisinopril", "10 mg", "Once daily", "30 days"),
            ("Hydrochlorothiazide", "25 mg", "Once daily", "30 days")
        ]);
        AddDrugs("Antidiabetic",
        [
            ("Metformin", "500 mg", "Twice daily", "30 days"),
            ("Glibenclamide", "5 mg", "Once daily", "30 days")
        ]);
        AddDrugs("Analgesic",
        [
            ("Paracetamol", "500 mg", "Three times daily", "5 days"),
            ("Ibuprofen", "400 mg", "Three times daily", "5 days")
        ]);
        AddDrugs("Antibiotic",
        [
            ("Amoxicillin", "500 mg", "Three times daily", "7 days"),
            ("Azithromycin", "500 mg", "Once daily", "3 days"),
            ("Ciprofloxacin", "500 mg", "Twice daily", "7 days")
        ]);
        AddDrugs("Supplement",
        [
            ("Folic Acid", "5 mg", "Once daily", "30 days"),
            ("Ferrous Sulphate + Folic Acid", "200/5 mg", "Once daily", "90 days"),
            ("Multivitamin", "One tablet", "Once daily", "30 days")
        ]);
        AddDrugs("Antenatal",
        [
            ("Tetanus Toxoid", "0.5 mL IM", "Single dose", "Once"),
            ("Ferrous Folate", "One tablet", "Once daily", "90 days")
        ]);
        AddDrugs("Antiglaucoma",
        [
            ("Timolol eye drops", "0.5%", "Twice daily", "30 days"),
            ("Latanoprost eye drops", "0.005%", "Once daily at night", "30 days")
        ]);

        return catalog;
    }
}
