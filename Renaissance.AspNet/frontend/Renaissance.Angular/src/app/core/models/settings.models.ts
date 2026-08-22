import { AppModule } from './auth.models';

export interface HospitalSettingsDto {
    facilityName: string;
    clientNumberPrefix: string;
    timeZoneId: string;
    updatedAtUtc?: string;
    updatedBy?: string;
}

export interface UpdateHospitalSettingsRequest {
    facilityName: string;
    clientNumberPrefix: string;
    timeZoneId: string;
}

export interface PublicHospitalSettingsDto {
    facilityName: string;
}

export interface LanSettingsDto {
    webAccessUrl: string;
    apiAccessUrl: string;
}

export interface UpdateLanSettingsRequest {
    webAccessUrl: string;
    apiAccessUrl: string;
}

export interface LanAccessStatusDto {
    isPasswordConfigured: boolean;
}

export interface LanAccessUnlockRequest {
    password: string;
}

export interface LanAccessUnlockResponse {
    token: string;
    expiresAtUtc: string;
}

export interface ChangeLanAccessPasswordRequest {
    currentPassword: string;
    newPassword: string;
}

export interface DeploymentInfoDto {
    localIpAddresses: string[];
    apiPort: number;
    webPort: number;
    bindAddress: string;
    allowLanAccess: boolean;
    lanReady: boolean;
    suggestedStaffUrl: string;
    suggestedApiUrl: string;
    staffAccessUrl: string;
    setupSteps: string[];
}

export interface ModuleServiceDescriptorDto {
    module: AppModule;
    name: string;
    description: string;
    category: string;
    isCore: boolean;
    isEnabled: boolean;
    supportsReferrals: boolean;
    sortOrder: number;
}

export interface ModuleReferralLinkDto {
    sourceModule: AppModule;
    targetModule: AppModule;
}

export interface HospitalModulesConfigDto {
    services: ModuleServiceDescriptorDto[];
    referralLinks: ModuleReferralLinkDto[];
    updatedAtUtc?: string;
    updatedBy?: string;
}

export interface UpdateHospitalModulesRequest {
    enabledModules: AppModule[];
    referralLinks: ModuleReferralLinkDto[];
}

export interface HospitalModulesStateDto {
    enabledModules: AppModule[];
    referralLinks: ModuleReferralLinkDto[];
}
