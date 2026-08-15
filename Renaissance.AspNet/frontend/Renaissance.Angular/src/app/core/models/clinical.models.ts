export interface AuditEntity {
    createdBy?: string;
    createdDate?: string;
    updatedBy?: string;
    updatedDate?: string;
    archived?: boolean;
}

export interface Patient extends AuditEntity {
    id: string;
    clientNumber: string;
    fullName?: string;
    age?: number;
    ageUnit?: string;
    sex?: string;
    maritalStatus?: string;
    tribe?: string;
    religion?: string;
    occupation?: string;
    education?: string;
    address?: string;
    phoneNumber?: string;
}

export interface Triage extends AuditEntity {
    id: string;
    patientId: string;
    diabetes?: boolean;
    asthma?: boolean;
    sickleCell?: boolean;
    smoking?: boolean;
    weight?: number;
    height?: number;
    temperature?: number;
    systolicBp?: number;
    diastolicBp?: number;
    pulseRate?: number;
    respiratoryRate?: number;
}

export interface Consultation extends AuditEntity {
    id: string;
    patientId: string;
    diagnoses?: string[];
    treatments?: string[];
    servicesReferred?: string[];
    othersDiagnosis?: string[];
    othersTreatment?: string[];
    referred?: boolean;
    itnOrder?: boolean;
    itnDispense?: boolean;
}

export interface PharmacyPrescription extends AuditEntity {
    id: string;
    patientId: string;
    drugCategory?: string;
    drugName?: string;
    dosage?: string;
    frequency?: string;
    duration?: string;
    dispensed?: boolean;
    dispensationNote?: string;
    status?: DispensationStatus;
    quantityDispensed?: number;
}

export enum DispensationStatus {
    Pending = 'PENDING',
    Dispensed = 'DISPENSED',
    Declined = 'DECLINED'
}

export interface Laboratory extends AuditEntity {
    id: string;
    patientId: string;
    testName?: string;
    result?: string;
    note?: string;
}

export interface DentalConsultation extends AuditEntity {
    id: string;
    patientId: string;
    diagnoses?: string[];
    treatments?: string[];
    dispensedItems?: string[];
    servicesReferred?: string[];
    othersDiagnosis?: string[];
    othersTreatment?: string[];
    referred?: boolean;
}

export interface Ancillary extends AuditEntity {
    id: string;
    patientId: string;
    services?: string[];
    pregnancyStatus?: string;
}

export interface Optometrist extends AuditEntity {
    id: string;
    patientId: string;
    visualAcuityRight?: string;
    visualAcuityLeft?: string;
    glassesDispensed?: boolean;
    referred?: boolean;
}

export interface Ophthalmologist extends AuditEntity {
    id: string;
    patientId: string;
    diagnoses?: string[];
    treatments?: string[];
    surgeries?: string[];
    othersDiagnosis?: string[];
    othersTreatment?: string[];
    otherSurgery?: string[];
    visualAcuityRight?: string;
    visualAcuityLeft?: string;
    glassesDispensed?: boolean;
    referred?: boolean;
}

export interface ClientDashboardDto {
    patient?: Patient;
    triages: Triage[];
    consultations: Consultation[];
    dentalConsultations: DentalConsultation[];
    laboratories: Laboratory[];
    pharmacyPrescriptions: PharmacyPrescription[];
    ancillaries: Ancillary[];
    optometrists: Optometrist[];
    ophthalmologists: Ophthalmologist[];
    hasPendingPharmacy: boolean;
}

export interface PharmacyDispenseUpdate {
    id: string;
    status: DispensationStatus;
    quantityDispensed?: number;
    dispensationNote?: string;
    dispensed: boolean;
}
