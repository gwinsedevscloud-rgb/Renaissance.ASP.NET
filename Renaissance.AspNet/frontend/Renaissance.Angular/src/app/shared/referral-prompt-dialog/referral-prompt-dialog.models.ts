import { AppModule } from '../../core/models/auth.models';
import { ReferralPriority } from '../../core/models/referral.models';

export interface ReferralPromptDialogData {
    patientId: string;
    patientName: string;
    sourceModule: AppModule;
    sourceRecordId?: string;
}

export type ReferralPromptDialogResult = 'skipped' | 'completed';

export interface ReferralPriorityOption {
    priority: ReferralPriority;
    label: string;
    hint: string;
    cssClass: string;
    icon: string;
}

export const REFERRAL_PRIORITY_OPTIONS: ReferralPriorityOption[] = [
    {
        priority: ReferralPriority.Emergency,
        label: 'Emergency',
        hint: 'Seen within 5 minutes',
        cssClass: 'priority-emergency',
        icon: 'heroicons_outline:bolt'
    },
    {
        priority: ReferralPriority.Urgent,
        label: 'Urgent',
        hint: 'Seen within 15 minutes',
        cssClass: 'priority-urgent',
        icon: 'heroicons_outline:exclamation-triangle'
    },
    {
        priority: ReferralPriority.Routine,
        label: 'Routine',
        hint: 'Standard queue · 30 min target',
        cssClass: 'priority-routine',
        icon: 'heroicons_outline:clock'
    }
];
