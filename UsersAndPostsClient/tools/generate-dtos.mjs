import fs from "node:fs";
import path from "node:path";

const root = process.cwd();

// Läs från backend-kontraktet (sibling folder)
const dtoPath = path.resolve(root, "../UsersAndPosts/dtos.json");
const outPath = path.resolve(root, "src/generated/dtos.ts");

const json = JSON.parse(fs.readFileSync(dtoPath, "utf-8"));

/**
 * dtos.json format:
 * {
 *   "User": { "UserDto": { "id": "int", ... }, "UserCreateDto": {...} },
 *   "Post": { ... }
 * }
 */

const typeMap = (t) => {
  switch (t) {
    case "int":
      return "number";
    case "string":
      return "string";
    case "string(date-time)":
      // Vi låter den vara string i TS (ISO). UI kan parse:a till Date vid behov.
      return "string";
    default:
      // fallback: okänt => unknown
      return "unknown";
  }
};

let out = `/* eslint-disable */
/**
 * AUTO-GENERATED FILE.
 * Source: ../UsersAndPosts/dtos.json
 * Do not edit manually.
 */
`;

out += "\n";

for (const [groupName, group] of Object.entries(json)) {
  out += `// ---- ${groupName} ----\n`;
  for (const [typeName, shape] of Object.entries(group)) {
    out += `export type ${typeName} = {\n`;
    for (const [prop, typ] of Object.entries(shape)) {
      out += `  ${prop}: ${typeMap(typ)};\n`;
    }
    out += `};\n\n`;
  }
}

fs.mkdirSync(path.dirname(outPath), { recursive: true });
fs.writeFileSync(outPath, out, "utf-8");

console.log(`Generated: ${path.relative(root, outPath)} from ${dtoPath}`);
