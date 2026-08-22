import { ReferralPriority, ReferralStatus } from '../../core/models/referral.models';

export function priorityLabel(priority: ReferralPriority): string {
    switch (priority) {
        case ReferralPriority.Emergency:
            return 'Emergency';
        case ReferralPriority.Urgent:
            return 'Urgent';
        default:
            return 'Routine';
    }
}

export function priorityClass(priority: ReferralPriority): string {
    switch (priority) {
        case ReferralPriority.Emergency:
            return 'priority-emergency';
        case ReferralPriority.Urgent:
            return 'priority-urgent';
        default:
            return 'priority-routine';
    }
}

export function statusLabel(status: ReferralStatus): string {
    switch (status) {
        case ReferralStatus.Pending:
            return 'Waiting';
        case ReferralStatus.InProgress:
            return 'In progress';
        case ReferralStatus.Completed:
            return 'Completed';
        case ReferralStatus.Cancelled:
            return 'Cancelled';
        default:
            return String(status);
    }
}

export function waitLabel(minutes: number): string {
    if (minutes < 1) {
        return 'Just arrived';
    }

    if (minutes < 60) {
        return `Waiting ${minutes} min`;
    }

    return `Waiting ${Math.floor(minutes / 60)}h ${minutes % 60}m`;
}
