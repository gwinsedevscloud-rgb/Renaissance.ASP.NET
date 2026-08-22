export function toMultiline(items?: string[] | null): string {
    if (!items?.length) return '';
    return items.join('\n');
}

export function fromMultiline(text?: string | null): string[] {
    if (!text?.trim()) return [];
    return text.split(/[\n,]+/).map(s => s.trim()).filter(Boolean);
}

export function clientDisplayName(fullName?: string | null, address?: string | null): string {
    if (fullName?.trim()) return fullName.trim();
    if (!address?.trim()) return '—';
    const comma = address.indexOf(',');
    return comma > 0 ? address.slice(0, comma).trim() : address.trim();
}

export function patientInitials(fullName?: string | null, address?: string | null, clientNumber?: string | null): string {
    const name = clientDisplayName(fullName, address);
    if (name !== '—') {
        const parts = name.split(/\s+/).filter(Boolean);
        if (parts.length >= 2) {
            return (parts[0][0] + parts[1][0]).toUpperCase();
        }
        return name.slice(0, 2).toUpperCase();
    }

    const num = clientNumber?.replace(/[^A-Z0-9]/gi, '') ?? '';
    return num.slice(0, 2).toUpperCase() || 'CL';
}
