// @ts-check
import {themes as prismThemes} from 'prism-react-renderer';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'Hamachi',
  tagline: 'Antweight melty-brain combat robot — Onshape model, firmware, build docs',
  favicon: 'img/favicon.ico',

  future: {
    v4: true,
  },

  url: 'https://griswaldbrooks.github.io',
  baseUrl: '/hamachi/',

  organizationName: 'griswaldbrooks',
  projectName: 'hamachi',

  onBrokenLinks: 'warn',

  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          path: '../docs',
          routeBasePath: '/',
          sidebarPath: './sidebars.js',
          exclude: ['fs/**', '**/*.fs'],
          editUrl: 'https://github.com/griswaldbrooks/hamachi/tree/main/',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      // image: 'img/social-card.jpg',  // TODO: add a hamachi social card
      colorMode: {
        respectPrefersColorScheme: true,
      },
      navbar: {
        title: 'Hamachi',
        logo: {
          alt: 'Hamachi',
          src: 'img/logo.svg',
        },
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'mainSidebar',
            position: 'left',
            label: 'Docs',
          },
          {
            href: 'https://github.com/griswaldbrooks/hamachi',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Docs',
            items: [
              {label: 'Onshape model', to: '/onshape-model-overview'},
              {label: 'Build guide', to: '/build-assembly-guide'},
              {label: 'Firmware setup', to: '/firmware-setup'},
              {label: 'Onshape-MCP cookbook (for agents)', to: '/onshape-mcp-cookbook'},
            ],
          },
          {
            title: 'Reference',
            items: [
              {label: 'Roadmap', to: '/roadmap'},
              {label: 'Decisions (ADRs)', to: '/decisions'},
              {label: 'Onshape migration log', to: '/onshape-migration'},
            ],
          },
          {
            title: 'Source',
            items: [
              {label: 'GitHub', href: 'https://github.com/griswaldbrooks/hamachi'},
              {label: 'Upstream (openmelt2)', href: 'https://github.com/nothinglabs/openmelt2'},
            ],
          },
        ],
        copyright: `© ${new Date().getFullYear()} Griswald Brooks. Hard fork of nothinglabs/openmelt2 (CC BY-NC-SA 4.0).`,
      },
      prism: {
        theme: prismThemes.github,
        darkTheme: prismThemes.dracula,
        additionalLanguages: ['bash', 'json', 'python'],
      },
    }),
};

export default config;
