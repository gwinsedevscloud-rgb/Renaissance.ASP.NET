export enum ExportRecordModule {
    Clients = 1,
    Triage = 2,
    Consultations = 3,
    Pharmacy = 4,
    Laboratory = 5,
    Dental = 6,
    Ancillary = 7,
    Optometrists = 8,
    Ophthalmologists = 9,
    Referrals = 10
}

export interface ExportModuleInfoDto {
    module: ExportRecordModule;
    name: string;
    description: string;
}

export interface ExportRecordsRequest {
    modules: ExportRecordModule[];
    fromDate?: string;
    toDate?: string;
    includeArchived: boolean;
    clientSearch?: string;
}

export interface ExportModuleCountDto {
    module: ExportRecordModule;
    name: string;
    count: number;
}

export interface ExportPreviewDto {
    modules: ExportModuleCountDto[];
    totalRecords: number;
}

export interface ExportDownloadResult {
    content: ArrayBuffer;
    fileName: string;
}
