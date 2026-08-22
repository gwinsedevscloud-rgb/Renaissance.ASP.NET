export enum AppModule {
    Clients = 1,
    Triage = 2,
    Consultations = 3,
    Pharmacy = 4,
    Laboratory = 5,
    Dental = 6,
    Ancillary = 7,
    Optometrists = 8,
    Ophthalmologists = 9,
    ClientDashboard = 10,
    Stakeholders = 11,
    Administration = 12
}

export interface LoginRequest {
    userName: string;
    password: string;
}

export interface LoginResponse {
    token: string;
    user: CurrentUserDto;
}

export interface CurrentUserDto {
    id: string;
    userName: string;
    fullName: string;
    roleId: string;
    roleName: string;
    isAdministrator: boolean;
    modules: AppModule[];
}

export interface UserDto {
    id: string;
    userName: string;
    fullName: string;
    roleId: string;
    roleName: string;
    isActive: boolean;
    createdDate?: string;
    modules: AppModule[];
    hasDirectModuleAccess: boolean;
}

export interface CreateUserRequest {
    userName: string;
    fullName: string;
    password: string;
    roleId: string;
    isActive: boolean;
    modules: AppModule[];
}

export interface UpdateUserRequest {
    fullName: string;
    password?: string;
    roleId: string;
    isActive: boolean;
    modules: AppModule[];
}

export interface RoleDto {
    id: string;
    name: string;
    description?: string;
    isSystem: boolean;
    userCount: number;
    modules: AppModule[];
}

export interface SaveRoleRequest {
    name: string;
    description?: string;
    modules: AppModule[];
}

export interface ModuleDescriptorDto {
    module: AppModule;
    name: string;
    description: string;
}

export interface ModuleNavItem {
    module: AppModule;
    label: string;
    href: string;
    icon: string;
    description: string;
}

export const MODULE_NAV: ModuleNavItem[] = [
    { module: AppModule.Clients, label: 'Patients', href: '/patients', icon: 'heroicons_outline:users', description: 'Register and manage patients' },
    { module: AppModule.Triage, label: 'Triage', href: '/triage', icon: 'heroicons_outline:heart', description: 'Vitals and medical history' },
    { module: AppModule.Consultations, label: 'Consultations', href: '/consultations', icon: 'heroicons_outline:clipboard-document-list', description: 'Diagnosis and treatment' },
    { module: AppModule.Pharmacy, label: 'Pharmacy', href: '/pharmacy', icon: 'heroicons_outline:beaker', description: 'Prescribe and dispense' },
    { module: AppModule.Laboratory, label: 'Laboratory', href: '/laboratory', icon: 'heroicons_outline:beaker', description: 'Tests and results' },
    { module: AppModule.Dental, label: 'Dental', href: '/dental', icon: 'heroicons_outline:face-smile', description: 'Dental consultations' },
    { module: AppModule.Ancillary, label: 'Ancillary', href: '/ancillary', icon: 'heroicons_outline:shield-check', description: 'Ancillary services' },
    { module: AppModule.Optometrists, label: 'Optometrists', href: '/optometrists', icon: 'heroicons_outline:eye', description: 'Optometry exams' },
    { module: AppModule.Ophthalmologists, label: 'Ophthalmologists', href: '/ophthalmologists', icon: 'heroicons_outline:eye', description: 'Ophthalmology care' },
    { module: AppModule.ClientDashboard, label: 'Patient Dashboard', href: '/patients', icon: 'heroicons_outline:squares-2x2', description: 'Per-patient clinical hub' },
    { module: AppModule.Stakeholders, label: 'Stakeholders', href: '/stakeholders', icon: 'heroicons_outline:chart-bar', description: 'KPIs and analytics' },
    { module: AppModule.Administration, label: 'Administration', href: '/admin/users', icon: 'heroicons_outline:shield-check', description: 'Users, roles, settings, backups, and module access' }
];

export const MODULE_NAV_CLINICAL: ModuleNavItem[] = MODULE_NAV.filter(item =>
    item.module === AppModule.Triage
    || item.module === AppModule.Consultations
    || item.module === AppModule.Pharmacy
    || item.module === AppModule.Laboratory
    || item.module === AppModule.Dental
    || item.module === AppModule.Ancillary
    || item.module === AppModule.Optometrists
    || item.module === AppModule.Ophthalmologists
);

export const MODULE_NAV_REFERRAL_TARGETS: ModuleNavItem[] = MODULE_NAV_CLINICAL.filter(
    item => item.module !== AppModule.Triage
);

export function displayModuleName(module: AppModule): string {
    return MODULE_NAV.find(m => m.module === module)?.label ?? AppModule[module];
}

export function moduleForPath(path: string): AppModule | null {
    const normalized = path.replace(/^\//, '');

    if (!normalized
        || normalized.startsWith('login')
        || normalized.startsWith('access-denied')
        || normalized.startsWith('not-found')
        || normalized.startsWith('Error')) {
        return null;
    }

    if (normalized.startsWith('patients') || normalized.startsWith('clients')) return AppModule.Clients;
    if (normalized.startsWith('patient-dashboard') || normalized.startsWith('client-dashboard')) return AppModule.ClientDashboard;
    if (normalized.startsWith('triage')) return AppModule.Triage;
    if (normalized.startsWith('consultations')) return AppModule.Consultations;
    if (normalized.startsWith('pharmacy')) return AppModule.Pharmacy;
    if (normalized.startsWith('laboratory')) return AppModule.Laboratory;
    if (normalized.startsWith('dental')) return AppModule.Dental;
    if (normalized.startsWith('ancillary')) return AppModule.Ancillary;
    if (normalized.startsWith('optometrists')) return AppModule.Optometrists;
    if (normalized.startsWith('ophthalmologists')) return AppModule.Ophthalmologists;
    if (normalized.startsWith('stakeholders')) return AppModule.Stakeholders;
    if (normalized.startsWith('admin') || normalized.startsWith('users') || normalized.startsWith('roles')) {
        return AppModule.Administration;
    }

    return null;
}

export function createHref(module: AppModule, patientId: string): string {
    const item = MODULE_NAV_CLINICAL.find(m => m.module === module);
    return item ? `${item.href}/create/${patientId}` : '/';
}

export function queueActionHref(module: AppModule, patientId: string): string {
    return module === AppModule.Pharmacy
        ? `/pharmacy/dispense/${patientId}`
        : createHref(module, patientId);
}
