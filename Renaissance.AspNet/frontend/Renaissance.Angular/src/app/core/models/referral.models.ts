import { AppModule } from './auth.models';

export enum ReferralStatus {
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

export enum ReferralPriority {
    Emergency = 1,
    Urgent = 2,
    Routine = 3
}

export interface CreateReferralsRequest {
    patientId: string;
    sourceModule: AppModule;
    sourceRecordId?: string;
    targetModules: AppModule[];
    notes?: string;
    priority: ReferralPriority;
}

export interface ReassignReferralRequest {
    targetModule: AppModule;
    notes?: string;
}

export interface ReferralQueueItem {
    id: string;
    patientId: string;
    patientFullName: string;
    clientNumber: string;
    patientAge?: number;
    patientAgeUnit?: string;
    patientSex?: string;
    sourceModule: AppModule;
    targetModule: AppModule;
    status: ReferralStatus;
    priority: ReferralPriority;
    notes?: string;
    referredBy?: string;
    attendedBy?: string;
    completedBy?: string;
    createdDate?: string;
    attendedDate?: string;
    completedDate?: string;
    queuePosition: number;
    waitMinutes: number;
    isSlaBreached: boolean;
}

export interface ReferralNotification {
    referralId: string;
    patientId: string;
    patientFullName: string;
    clientNumber: string;
    sourceModule: AppModule;
    targetModule: AppModule;
    priority: ReferralPriority;
    notes?: string;
    createdDate?: string;
}

export interface ReferralInboxItem {
    referralId: string;
    patientId: string;
    patientFullName: string;
    clientNumber: string;
    sourceModule: AppModule;
    targetModule: AppModule;
    status: ReferralStatus;
    priority: ReferralPriority;
    notes?: string;
    createdDate?: string;
    waitMinutes: number;
    isRead: boolean;
}

export interface ModuleReferralCount {
    module: AppModule;
    pendingCount: number;
}

export interface PatientJourneyStep {
    referralId: string;
    targetModule: AppModule;
    sourceModule: AppModule;
    status: ReferralStatus;
    priority: ReferralPriority;
    referredBy?: string;
    attendedBy?: string;
    completedBy?: string;
    createdDate?: string;
    attendedDate?: string;
    completedDate?: string;
    notes?: string;
}

export interface CreateReferralsResult {
    created: ReferralQueueItem[];
    skippedDuplicates: AppModule[];
}
