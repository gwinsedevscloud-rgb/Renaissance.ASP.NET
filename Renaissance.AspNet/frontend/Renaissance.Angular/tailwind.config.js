const path = require('path');
const colors = require('tailwindcss/colors');
const defaultTheme = require('tailwindcss/defaultTheme');
const { withMaterialColors } = require('tailwind-material-colors');

const PRIMARY = '#0d6e6e';
const TERTIARY = '#e6f4f4';

const config = {
    darkMode: 'class',
    content: ['./src/**/*.{html,scss,ts}'],
    important: true,
    theme: {
        fontSize: {
            xs: '0.625rem', sm: '0.75rem', md: '0.8125rem', base: '0.875rem',
            lg: '1rem', xl: '1.125rem', '2xl': '1.25rem', '3xl': '1.5rem',
            '4xl': '2rem', '5xl': '2.25rem', '6xl': '2.5rem', '7xl': '3rem',
            '8xl': '4rem', '9xl': '6rem', '10xl': '8rem'
        },
        screens: { sm: '600px', md: '960px', lg: '1280px', xl: '1440px' },
        extend: {
            colors: {
                gray: colors.gray, slate: colors.slate, teal: colors.teal,
                cyan: colors.cyan, blue: colors.blue, emerald: colors.emerald
            },
            fontFamily: {
                sans: `"Inter var", ${defaultTheme.fontFamily.sans.join(',')}`
            },
            zIndex: { '49': 49, '60': 60, '99': 99, '999': 999 }
        }
    },
    corePlugins: {
        appearance: false, container: false, float: false, clear: false,
        placeholderColor: false, placeholderOpacity: false, verticalAlign: false
    },
    plugins: [
        require(path.resolve(__dirname, './tailwind/plugins/icon-size')),
        require(path.resolve(__dirname, './tailwind/plugins/fuse'))(),
        require('@tailwindcss/typography')({ modifiers: ['sm', 'lg'] })
    ]
};

module.exports = withMaterialColors(config, {
    primary: PRIMARY,
    tertiary: TERTIARY,
    teal: colors.teal[500],
    emerald: colors.emerald[500],
    blue: colors.blue[500]
}, { scheme: 'content', contrast: 0 });
