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
