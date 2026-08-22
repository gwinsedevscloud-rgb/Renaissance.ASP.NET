export interface BackupInfoDto {
    fileName: string;
    sizeBytes: number;
    createdAtUtc: string;
    sizeLabel: string;
}

export interface BackupCreateResultDto {
    backup: BackupInfoDto;
    message: string;
}

export interface BackupRestoreResultDto {
    message: string;
    patientsRestored: number;
    usersRestored: number;
}
