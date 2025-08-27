import { Component, effect, inject, signal } from '@angular/core';
import * as colors from 'tailwindcss/colors';
import {
    argbFromHex,
    CustomColor,
    customColor,
    DynamicScheme,
    Hct,
    hexFromArgb,
    MaterialDynamicColors,
    SchemeContent,
    SchemeExpressive,
    SchemeFidelity,
    SchemeMonochrome,
    SchemeNeutral,
    SchemeTonalSpot,
    SchemeVibrant,
    themeFromSourceColor
} from '@material/material-color-utilities';
import * as Color from 'color';
import { DOCUMENT, NgTemplateOutlet } from '@angular/common';
import { MatIconButton } from '@angular/material/button';

const DYNAMIC_THEME_STYLESHEET_ID = 'tailwind-material-colors-dynamic-theme';

const DEFAULT_COLOR = '#212a31';

type WithStylesheet = typeof globalThis & {
    [stylesheetName: string]: CSSStyleSheet | undefined;
};

const materialDynamicColors = {
    background: MaterialDynamicColors.background,
    'on-background': MaterialDynamicColors.onBackground,

    surface: MaterialDynamicColors.surface,
    'surface-dim': MaterialDynamicColors.surfaceDim,
    'surface-bright': MaterialDynamicColors.surfaceBright,
    'surface-container-lowest': MaterialDynamicColors.surfaceContainerLowest,
    'surface-container-low': MaterialDynamicColors.surfaceContainerLow,
    'surface-container': MaterialDynamicColors.surfaceContainer,
    'surface-container-high': MaterialDynamicColors.surfaceContainerHigh,
    'surface-container-highest': MaterialDynamicColors.surfaceContainerHighest,
    'on-surface': MaterialDynamicColors.onSurface,
    'on-surface-dim': MaterialDynamicColors.onSurface,
    'on-surface-bright': MaterialDynamicColors.onSurface,
    'on-surface-container-lowest': MaterialDynamicColors.onSurface,
    'on-surface-container-low': MaterialDynamicColors.onSurface,
    'on-surface-container': MaterialDynamicColors.onSurface,
    'on-surface-container-high': MaterialDynamicColors.onSurface,
    'on-surface-container-highest': MaterialDynamicColors.onSurface,

    'surface-variant': MaterialDynamicColors.surfaceVariant,
    'on-surface-variant': MaterialDynamicColors.onSurfaceVariant,

    'inverse-surface': MaterialDynamicColors.inverseSurface,
    'on-inverse-surface': MaterialDynamicColors.inverseOnSurface,

    outline: MaterialDynamicColors.outline,
    'outline-variant': MaterialDynamicColors.outlineVariant,

    'surface-tint': MaterialDynamicColors.surfaceTint,
    'on-surface-tint': MaterialDynamicColors.onSurface,

    primary: MaterialDynamicColors.primary,
    'on-primary': MaterialDynamicColors.onPrimary,

    'primary-container': MaterialDynamicColors.primaryContainer,
    'on-primary-container': MaterialDynamicColors.onPrimaryContainer,

    'inverse-primary': MaterialDynamicColors.inversePrimary,

    'primary-fixed': MaterialDynamicColors.primaryFixed,
    'primary-fixed-dim': MaterialDynamicColors.primaryFixedDim,
    'on-primary-fixed': MaterialDynamicColors.onPrimaryFixed,
    'on-primary-fixed-variant': MaterialDynamicColors.onPrimaryFixedVariant,

    secondary: MaterialDynamicColors.secondary,
    'on-secondary': MaterialDynamicColors.onSecondary,

    'secondary-container': MaterialDynamicColors.secondaryContainer,
    'on-secondary-container': MaterialDynamicColors.onSecondaryContainer,

    'secondary-fixed': MaterialDynamicColors.secondaryFixed,
    'secondary-fixed-dim': MaterialDynamicColors.secondaryFixedDim,
    'on-secondary-fixed': MaterialDynamicColors.onSecondaryFixed,
    'on-secondary-fixed-variant': MaterialDynamicColors.onSecondaryFixedVariant,

    tertiary: MaterialDynamicColors.tertiary,
    'on-tertiary': MaterialDynamicColors.onTertiary,

    'tertiary-container': MaterialDynamicColors.tertiaryContainer,
    'on-tertiary-container': MaterialDynamicColors.onTertiaryContainer,

    'tertiary-fixed': MaterialDynamicColors.tertiaryFixed,
    'tertiary-fixed-dim': MaterialDynamicColors.tertiaryFixedDim,
    'on-tertiary-fixed': MaterialDynamicColors.onTertiaryFixed,
    'on-tertiary-fixed-variant': MaterialDynamicColors.onTertiaryFixed,

    error: MaterialDynamicColors.error,
    'on-error': MaterialDynamicColors.onError,

    'error-container': MaterialDynamicColors.errorContainer,
    'on-error-container': MaterialDynamicColors.onErrorContainer
};

@Component({
    selector: 'color-scheme',
    template: `
        <!-- Button -->
        <button
                (click)="toggleScheme()"
                mat-icon-button>
            <ng-container *ngTemplateOutlet="schemeColor; context: {$implicit: isDark()}"></ng-container>
        </button>
        <!-- Flag image template -->
        <ng-template
                #schemeColor
                let-dark>
            <span class="relative w-6 shadow rounded-sm overflow-hidden">
                @if (dark === false) {
                    <svg width="800px" height="800px" viewBox="0 0 48 48" id="b" xmlns="http://www.w3.org/2000/svg">
                        <defs>
                            <style>.c {
                                fill: #000000;
                                stroke: #000000;
                                stroke-linecap: round;
                                stroke-linejoin: round;
                            }
                            </style>
                        </defs>
                        <path class="c"
                            d="m32.8,29.3c-8.9-.8-16.2-7.8-17.5-16.6-.3-1.8-.3-3.7,0-5.4.2-1.4-1.4-2.3-2.5-1.6C6.3,9.7,2.1,16.9,2.5,25c.5,10.7,9,19.5,19.7,20.4,10.6.9,19.8-6,22.5-15.6.4-1.4-1-2.6-2.3-2-2.9,1.3-6.1,1.8-9.6,1.5Z"/>
                    </svg>
                } @else {
                    <svg width="800px" height="800px" viewBox="0 0 24 24" fill="none"
                         xmlns="http://www.w3.org/2000/svg">
                        <g clip-path="url(#a)" stroke="#ffffff" stroke-width="1.5" stroke-miterlimit="10">
                            <path d="M5 12H1M23 12h-4M7.05 7.05 4.222 4.222M19.778 19.778 16.95 16.95M7.05 16.95l-2.828 2.828M19.778 4.222 16.95 7.05"
                                  stroke-linecap="round"/>
                            <path d="M12 16a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z" fill="#ffffff" fill-opacity=".5"/>
                            <path d="M12 19v4M12 1v4" stroke-linecap="round"/>
                        </g>
                        <defs>
                            <clipPath id="a">
                                <path fill="#000000" d="M0 0h24v24H0z"/>
                            </clipPath>
                        </defs>
                    </svg>
                }
            </span>
        </ng-template>
    `,
    imports: [
        MatIconButton,
        NgTemplateOutlet
    ],
})
export class ColorSchemeComponent {
    colors = {
        gray: colors.gray[500],
        slate: colors.slate[500],
        zinc: colors.zinc[500],
        neutral: colors.neutral[500],
        stone: colors.stone[500],
        red: colors.red[500],
        orange: colors.orange[500],
        amber: colors.amber[500],
        yellow: colors.yellow[500],
        lime: colors.lime[500],
        green: colors.green[500],
        emerald: colors.emerald[500],
        teal: colors.teal[500],
        cyan: colors.cyan[500],
        sky: colors.sky[500],
        blue: colors.blue[500],
        indigo: colors.indigo[500],
        violet: colors.violet[500],
        purple: colors.purple[500],
        fuchsia: colors.fuchsia[500],
        pink: colors.pink[500],
        rose: colors.rose[500]
    }
    isDark = signal(false);
    #document = inject(DOCUMENT);

    constructor() {
        effect(() => {
            this.updateScheme(this.isDark());
        });
    }

    toggleScheme() {
        this.isDark.update(prev => !prev)
    }

    updateScheme(isDark: boolean): void {

        const lightScheme = new SchemeContent(Hct.fromInt(argbFromHex(DEFAULT_COLOR)), false, 0);
        const darkScheme = new SchemeContent(Hct.fromInt(argbFromHex(DEFAULT_COLOR)), true, 0);

        let tertiaryPalette = lightScheme.tertiaryPalette;

        this.createCustomProperties(isDark, DEFAULT_COLOR, lightScheme, darkScheme);
        this.updateTailwind(DEFAULT_COLOR);

        this.#document.body.classList.remove('light', 'dark');

        // Add class name for the currently selected scheme
        this.#document.body.classList.add(isDark ? 'dark' : 'light');
    }

    createCustomProperties(dark: boolean, primary: string, lightScheme: DynamicScheme, darkScheme: DynamicScheme) {
        let styleString = ':root .light {';

        Object.entries(materialDynamicColors).forEach(([name, DynamicColor]) => {
            const hex = hexFromArgb(DynamicColor.getArgb(lightScheme));
            styleString += `--mat-sys-${name}:${hex};`;
        });

        let customColors: CustomColor[] = Object.entries(this.colors).map((color) => {
                return {
                    name: color[0],
                    value: argbFromHex(color[1]),
                    blend: true
                }
            }
        );
        let customPalette = themeFromSourceColor(argbFromHex(primary), customColors);
        customPalette.customColors.map(cg => {
            styleString += `--${cg.color.name}:${hexFromArgb(cg.light.color)};`;
            styleString += `--on-${cg.color.name}:${hexFromArgb(cg.light.onColor)};`;
            styleString += `--${cg.color.name}-container:${hexFromArgb(cg.light.colorContainer)};`;
            styleString += `--on-${cg.color.name}-container:${hexFromArgb(cg.light.onColorContainer)};`;

        })
        styleString += '}';
        if (!dark) {
            this.applyThemeString(styleString, 'angular-material-theme');
        }

        styleString = ':host, :root .dark {';
        Object.entries(materialDynamicColors).forEach(([name, DynamicColor]) => {
            const hex = hexFromArgb(DynamicColor.getArgb(darkScheme));
            styleString += `--mat-sys-${name}:${hex};`;
        });

        customColors = Object.entries(this.colors).map((color) => {
                return {
                    name: color[0],
                    value: argbFromHex(color[1]),
                    blend: true
                }
            }
        );
        customPalette = themeFromSourceColor(argbFromHex(primary), customColors);
        customPalette.customColors.map(cg => {
            styleString += `--${cg.color.name}:${hexFromArgb(cg.dark.color)};`;
            styleString += `--on-${cg.color.name}:${hexFromArgb(cg.dark.onColor)};`;
            styleString += `--${cg.color.name}-container:${hexFromArgb(cg.dark.colorContainer)};`;
            styleString += `--on-${cg.color.name}-container:${hexFromArgb(cg.dark.onColorContainer)};`;
        })
        styleString += '}';

        if (dark) {
            this.applyThemeString(styleString, 'angular-material-theme');
        }
    }

    applyThemeString(themeString: string, ssName = 'angular-material-theme') {
        let sheet = (globalThis as WithStylesheet)[ssName];

        if (!sheet) {
            sheet = new CSSStyleSheet();
            (globalThis as WithStylesheet)[ssName] = sheet;
            this.#document.adoptedStyleSheets.push(sheet);
        }
        sheet.replaceSync(themeString);
    }

    updateTailwind(primaryColor: string) {
        const colors: any = {
            primary: primaryColor,
            ...this.colors
        };

        this.updateTheme(colors, 'class', 'content', 0);

    }

    tailwindThemeFromColor = (colorsMap: { [x: string]: any; primary: any; }, scheme: string, contrast: number) => {
        const {primary, ...extraColors} = colorsMap;

        const source = argbFromHex(primary);

        const SchemeObjects = {
            content: SchemeContent,
            expressive: SchemeExpressive,
            fidelity: SchemeFidelity,
            monochrome: SchemeMonochrome,
            neutral: SchemeNeutral,
            tonalSpot: SchemeTonalSpot,
            vibrant: SchemeVibrant,
        };
        // @ts-ignore
        const SchemeObject = SchemeObjects[scheme] || SchemeObjects.content;

        const lightScheme = new SchemeObject(Hct.fromInt(source), false, contrast);
        const darkScheme = new SchemeObject(Hct.fromInt(source), true, contrast);

        // default colors
        const colors: { [key: string]: any } = {
            transparent: 'transparent',
            current: 'currentColor',
            black: '#000000',
            white: '#ffffff',
        };

        Object.entries(materialDynamicColors).forEach(([name, DynamicColor]) => {
            const lightHex = hexFromArgb(DynamicColor.getArgb(lightScheme));
            const darkHex = hexFromArgb(DynamicColor.getArgb(darkScheme));
            colors[`${name}-light`] = lightHex;
            colors[`${name}-dark`] = darkHex;
        });

        Object.keys(extraColors).forEach((colorName) => {
            const value = extraColors[colorName];

            const hex = typeof value === 'string' ? value : value.hex;
            const blend = value === hex ? true : value.harmonize;

            const {light, dark} = customColor(source, {
                value: argbFromHex(hex),
                blend,
                name: ''
            });
            const kebabName = this.kebabize(colorName);

            colors[`${kebabName}-light`] = hexFromArgb(light.color);
            colors[`on-${kebabName}-light`] = hexFromArgb(light.onColor);
            colors[`${kebabName}-container-light`] = hexFromArgb(light.colorContainer);
            colors[`on-${kebabName}-container-light`] = hexFromArgb(
                light.onColorContainer
            );
            colors[`${kebabName}-dark`] = hexFromArgb(dark.color);
            colors[`on-${kebabName}-dark`] = hexFromArgb(dark.onColor);
            colors[`${kebabName}-container-dark`] = hexFromArgb(dark.colorContainer);
            colors[`on-${kebabName}-container-dark`] = hexFromArgb(
                dark.onColorContainer
            );
        });
        return colors;
    };

    kebabize = (str: string) => {
        return str
            .split('')
            .map((letter: string, idx: number) => {
                return letter.toUpperCase() === letter
                    ? `${idx !== 0 ? '-' : ''}${letter.toLowerCase()}`
                    : letter;
            })
            .join('');
    };

    updateTheme = (colorsMap: {
        [x: string]: any;
        primary: any;
    }, darkModeConfig = 'media', scheme = 'content', contrast = 0) => {
        let newCSS = '';

        const LIGHT_SELECTOR = ':root';
        const DARK_SELECTOR = Array.isArray(darkModeConfig)
            ? darkModeConfig[0] === 'class'
                ? darkModeConfig[1] || '.dark'
                : '@media (prefers-color-scheme: dark)'
            : darkModeConfig === 'class'
                ? '.dark'
                : '@media (prefers-color-scheme: dark)';

        if (colorsMap.primary) {
            const {black, white, transparent, current, ...colors} =
                this.tailwindThemeFromColor(colorsMap, scheme, contrast);

            Object.keys(colors).forEach((lightColorName) => {
                const match = lightColorName.match(
                    new RegExp(/^(?:(.+)-)?light(?:-(.+))?$/)
                );
                if (match) {
                    const prefix = match[1];
                    const suffix = match[2];

                    const modeAwareColorName = [prefix, suffix].filter((x) => x).join('-');
                    const darkColorName = [prefix, 'dark', suffix]
                        .filter((x) => x)
                        .join('-');

                    const lightColor = colors[lightColorName];
                    const darkColor = colors[darkColorName];

                    if (lightColor && darkColor) {
                        newCSS += `
                              ${LIGHT_SELECTOR} {
                                --color-${modeAwareColorName}: ${Color(lightColor)
                            .rgb()
                            .array()
                            .join(' ')};
                              }
                              ${DARK_SELECTOR} {
                                --color-${modeAwareColorName}: ${Color(darkColor)
                            .rgb()
                            .array()
                            .join(' ')};
                              }
                        `;
                    }
                }
            });

            newCSS.replace(/\s*\n\s*/g, ' ');

            let stylesheet = this.#document.getElementById(DYNAMIC_THEME_STYLESHEET_ID);
            if (!stylesheet) {
                stylesheet = this.#document.createElement('style');
                stylesheet.id = DYNAMIC_THEME_STYLESHEET_ID;
                this.#document.head.appendChild(stylesheet);
            }
            stylesheet.innerText = newCSS;
        } else {
            throw 'A primary color must be specified';
        }
    };
}
