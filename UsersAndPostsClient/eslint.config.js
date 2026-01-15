import js from "@eslint/js";
import globals from "globals";
import react from "eslint-plugin-react";
import reactHooks from "eslint-plugin-react-hooks";
import tseslint from "@typescript-eslint/eslint-plugin";
import tsParser from "@typescript-eslint/parser";

/**
 * ESLint för JS + TS + React
 * - Fokus på riktiga fel
 * - Ingen whitespace / formattering
 * - Säker för CI (check-mode)
 */
export default [
  // Basregler för JavaScript
  js.configs.recommended,

  {
    files: ["**/*.{js,jsx,ts,tsx}"],

    languageOptions: {
      parser: tsParser, // 👈 detta gör att TS förstås korrekt
      ecmaVersion: "latest",
      sourceType: "module",
      globals: {
        ...globals.browser,
        ...globals.node,
      },
    },

    plugins: {
      react,
      "react-hooks": reactHooks,
      "@typescript-eslint": tseslint,
    },

    settings: {
      react: {
        version: "detect",
      },
    },

    rules: {
      /* =========================
         ✅ GENERELLA KVALITETSREGLER
         ========================= */

      // Disable no-undef for TypeScript since TS handles it better
      "no-undef": "off",

      "eqeqeq": ["error", "smart"],

      // JS-versionen stängs av till förmån för TS-varianten
      "no-unused-vars": "off",
      "@typescript-eslint/no-unused-vars": [
        "warn",
        {
          argsIgnorePattern: "^_",
          varsIgnorePattern: "^_",
        },
      ],

      /* =========================
         ⚛️ REACT & HOOKS
         ========================= */

      "react/react-in-jsx-scope": "off",
      "react-hooks/rules-of-hooks": "error",
      "react-hooks/exhaustive-deps": "warn",

      /* =========================
         🧠 TYPESCRIPT (SNÄLL, MEN NYTTIG)
         ========================= */

      // Tillåt any i undervisning (men varna)
      "@typescript-eslint/no-explicit-any": "warn",

      // Förhindrar farliga konstruktioner
      "@typescript-eslint/no-empty-function": "warn",

      /* =========================
         🚫 STILREGLER – AVSTÄNGDA
         ========================= */

      "indent": "off",
      "semi": "off",
      "quotes": "off",
      "comma-dangle": "off",
      "object-curly-spacing": "off",
      "brace-style": "off",
      "keyword-spacing": "off",
      "space-before-function-paren": "off",
      "arrow-spacing": "off",
      "eol-last": "off",

      "react/jsx-indent": "off",
      "react/jsx-indent-props": "off",
      "react/jsx-curly-spacing": "off",
    },
  },

  // Config for .mjs files (Node.js ES modules)
  {
    files: ["**/*.mjs"],
    languageOptions: {
      ecmaVersion: "latest",
      sourceType: "module",
      globals: {
        ...globals.node,
      },
    },
    rules: {
      // Keep basic rules, but allow Node globals
    },
  },
];
