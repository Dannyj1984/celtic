import type { Config } from 'tailwindcss'

export default <Partial<Config>>{
  content: [
    "./app/components/**/*.{vue,js,ts}",
    "./app/layouts/**/*.vue",
    "./app/pages/**/*.vue",
    "./app/app.vue",
    "./app/error.vue"
  ],
  safelist: [
    {
      pattern: /^(bg|text|border|ring)-(primary|secondary|tertiary|neutral)-(50|100|200|300|400|500|600|700|800|900|950)$/,
    }
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
      colors: {
        'primary': {
          50: '#f0f4f8',
          100: '#d1deeb',
          200: '#a3bdd7',
          300: '#759cc3',
          400: '#477aaf',
          500: '#0A1D37',
          600: '#08172c',
          700: '#061121',
          800: '#040b16',
          900: '#02060b',
          950: '#010305',
          DEFAULT: '#0A1D37',
        },
        'secondary': {
          50: '#fff3e6',
          100: '#ffe6cc',
          200: '#ffcd99',
          300: '#ffb466',
          400: '#ff9b33',
          500: '#FF6B00',
          600: '#cc5600',
          700: '#994000',
          800: '#662b00',
          900: '#331500',
          950: '#1a0b00',
          DEFAULT: '#FF6B00',
        },
        'tertiary': {
          50: '#e6fcff',
          100: '#ccf9ff',
          200: '#99f3ff',
          300: '#66edff',
          400: '#33e7ff',
          500: '#00E0FF',
          600: '#00b3cc',
          700: '#008699',
          800: '#005a66',
          900: '#002d33',
          950: '#00171a',
          DEFAULT: '#00E0FF',
        },
        'neutral': {
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748B',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
          950: '#020617',
          DEFAULT: '#64748B',
        },
        'celtic-green': '#1B5E20',
        'celtic-green-light': '#2E7D32',
        'celtic-green-dark': '#0D3B13',
        'celtic-gold': '#F9A825',
        'celtic-gold-light': '#FDD835',
        'surface': '#0F1419',
        'surface-light': '#1A2332',
        'surface-card': '#1E2A3A',
        'surface-hover': '#243447',
        'text-primary': '#F8FAFC',
        'text-secondary': '#CBD5E1',
        'text-tertiary': '#00E0FF',
        'text-neutral': '#94A3B8',
        'text-muted': '#64748B',
        'border': '#2D3748',
        'border-light': '#374151',
        'success': '#34D399',
        'warning': '#FBBF24',
        'danger': '#F87171',
        'info': '#60A5FA',
      },
      borderRadius: {
        'card': '12px',
        'button': '8px',
        'input': '8px',
      }
    }
  }
}
