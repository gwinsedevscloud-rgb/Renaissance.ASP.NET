export function listToText(values?: string[] | null): string {
    if (!values?.length) {
        return '';
    }

    return values.filter(v => v.trim()).join('\n');
}

export function textToList(text?: string | null): string[] {
    if (!text?.trim()) {
        return [];
    }

    return text
        .split(/[\r\n,]+/)
        .map(v => v.trim())
        .filter(v => v.length > 0);
}
