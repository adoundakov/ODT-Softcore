#!/usr/bin/env tsx

/**
 * ID Validation Script
 *
 * Extracts all MongoDB IDs from TypeScript asset files and maps them to
 * ItemTpl/BaseClasses/QuestTpl constants from SPT's server-csharp repository.
 * Names come from the server's English locale so the CSV is human-readable.
 *
 * Usage: npx tsx scripts/validate-ids.ts
 *        SPT_REF=4.1.5 npx tsx scripts/validate-ids.ts   # pin to a different tag/branch
 * Output: scripts/id-mapping.csv, scripts/id-mapping.json
 */

import fs from 'fs';
import path from 'path';

// MongoDB ID pattern: 24 character hexadecimal string
const MONGO_ID_REGEX = /["']([a-f0-9]{24})["']/gi;

// Git ref (tag or branch) of SP-Tushonka/server-csharp to read enums and locales from.
// Pin to the SPT version the C# mod targets; `main` is the next major and may not match.
const SPT_REF = process.env.SPT_REF ?? '4.1.5';
const SPT_RAW_BASE = `https://raw.githubusercontent.com/SP-Tushonka/server-csharp/${SPT_REF}`;
const SPT_ENUMS_PATH = 'Libraries/SPTushonka.Server.Core/Models/Enums';
const SPT_LOCALE_PATH = 'Libraries/SPTushonka.Server.Assets/SPT_Data/database/locales/global/en.json';

interface IdMapping {
  tsId: string;
  name: string;
  constant: string;
}

/**
 * Extract all MongoDB IDs from TypeScript files in src/assets/
 */
function extractMongoIds(): Set<string> {
  const assetsDir = path.join(process.cwd(), 'src', 'assets');
  const ids = new Set<string>();

  function processFile(filePath: string) {
    const content = fs.readFileSync(filePath, 'utf-8');
    const matches = content.matchAll(MONGO_ID_REGEX);

    for (const match of matches) {
      ids.add(match[1]);
    }
  }

  function walkDir(dir: string) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });

    for (const entry of entries) {
      const fullPath = path.join(dir, entry.name);

      if (entry.isDirectory()) {
        walkDir(fullPath);
      } else if (entry.isFile() && entry.name.endsWith('.ts')) {
        processFile(fullPath);
      }
    }
  }

  walkDir(assetsDir);
  console.log(`Found ${ids.size} unique MongoDB IDs in TypeScript files`);

  return ids;
}

/**
 * Fetch enum file from GitHub and parse MongoId constants
 */
async function fetchEnumMap(fileName: string, prefix: string): Promise<Map<string, string>> {
  const url = `${SPT_RAW_BASE}/${SPT_ENUMS_PATH}/${fileName}`;

  console.log(`Fetching ${fileName} from GitHub (${SPT_REF})...`);
  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`Failed to fetch ${fileName}: ${response.statusText}`);
  }

  const content = await response.text();
  const map = new Map<string, string>();

  // Parse lines like: public static readonly MongoId AMMO_127X108_B32 = new MongoId("5cde8864d7f00c0010373be1");
  // The generated enum files wrap long declarations across lines, so allow whitespace around the id.
  const enumRegex = /public static readonly MongoId (\w+) = new MongoId\(\s*"([a-f0-9]{24})"\s*\);/g;
  const matches = content.matchAll(enumRegex);

  for (const match of matches) {
    const [, constantName, mongoId] = match;
    map.set(mongoId, `${prefix}.${constantName}`);
  }

  console.log(`Parsed ${map.size} ${prefix} constants`);

  return map;
}

/**
 * Fetch all SPT enums and build combined reverse lookup map
 */
async function fetchAllEnums(): Promise<Map<string, string>> {
  const combined = new Map<string, string>();

  // Fetch each enum type
  const itemTplMap = await fetchEnumMap('ItemTpl.cs', 'ItemTpl');
  const baseClassesMap = await fetchEnumMap('BaseClasses.cs', 'BaseClasses');
  const questTplMap = await fetchEnumMap('QuestTpl.cs', 'QuestTpl');

  // Merge into combined map
  for (const [id, constant] of itemTplMap) {
    combined.set(id, constant);
  }
  for (const [id, constant] of baseClassesMap) {
    combined.set(id, constant);
  }
  for (const [id, constant] of questTplMap) {
    combined.set(id, constant);
  }

  console.log(`Total constants: ${combined.size}\n`);

  return combined;
}

/**
 * Fetch the server's English locale. Item names are keyed `<id> Name`;
 * handbook categories (which have no enum) are keyed by bare `<id>`.
 */
async function fetchLocale(): Promise<Record<string, string>> {
  const url = `${SPT_RAW_BASE}/${SPT_LOCALE_PATH}`;

  console.log(`Fetching en.json locale from GitHub (${SPT_REF})...`);
  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`Failed to fetch locale: ${response.statusText}`);
  }

  const locale = (await response.json()) as Record<string, string>;
  console.log(`Parsed ${Object.keys(locale).length} locale entries\n`);

  return locale;
}

function lookupName(id: string, locale: Record<string, string>): string {
  return locale[`${id} Name`] ?? locale[id] ?? 'UNKNOWN';
}

/**
 * Build complete mapping
 */
function buildMapping(ids: Set<string>, enumMap: Map<string, string>, locale: Record<string, string>): IdMapping[] {
  const mappings: IdMapping[] = [];

  for (const id of ids) {
    const constant = enumMap.get(id) || 'NOT_FOUND';
    mappings.push({ tsId: id, name: lookupName(id, locale), constant });
  }

  console.log(`Built ${mappings.length} mappings`);

  return mappings;
}

/**
 * Write mappings to CSV
 */
function writeCsv(mappings: IdMapping[], outputPath: string) {
  const header = 'TS_ID,Name,Constant\n';
  const rows = mappings
    .sort((a, b) => a.constant.localeCompare(b.constant))
    .map(m => `${m.tsId},"${m.name.replace(/"/g, '""')}",${m.constant}`)
    .join('\n');

  fs.writeFileSync(outputPath, header + rows, 'utf-8');
  console.log(`Wrote ${mappings.length} mappings to ${outputPath}`);
}

/**
 * Write mappings to JSON (for programmatic lookup during refactoring)
 */
function writeJson(mappings: IdMapping[], outputPath: string) {
  const lookup: Record<string, string> = {};

  for (const mapping of mappings) {
    lookup[mapping.tsId] = mapping.constant;
  }

  fs.writeFileSync(outputPath, JSON.stringify(lookup, null, 2), 'utf-8');
  console.log(`Wrote JSON lookup to ${outputPath}`);
}

/**
 * Print summary statistics
 */
function printSummary(mappings: IdMapping[]) {
  const notFound = mappings.filter(m => m.constant === 'NOT_FOUND').length;
  const byType: Record<string, number> = {};

  for (const mapping of mappings) {
    if (mapping.constant === 'NOT_FOUND') continue;
    const prefix = mapping.constant.split('.')[0];
    byType[prefix] = (byType[prefix] || 0) + 1;
  }

  console.log(`\nSummary:`);
  console.log(`  Total IDs: ${mappings.length}`);
  console.log(`  Mapped successfully: ${mappings.length - notFound}`);
  console.log(`  Not found in any enum: ${notFound}`);
  console.log(`\nBreakdown by type:`);
  for (const [type, count] of Object.entries(byType).sort((a, b) => b[1] - a[1])) {
    console.log(`  ${type}: ${count}`);
  }
}

/**
 * Main execution
 */
async function main() {
  try {
    console.log('Starting ID validation...\n');

    const ids = extractMongoIds();
    const enumMap = await fetchAllEnums();
    const locale = await fetchLocale();
    const mappings = buildMapping(ids, enumMap, locale);

    const csvPath = path.join(process.cwd(), 'scripts', 'id-mapping.csv');
    const jsonPath = path.join(process.cwd(), 'scripts', 'id-mapping.json');

    writeCsv(mappings, csvPath);
    writeJson(mappings, jsonPath);
    printSummary(mappings);

  } catch (error) {
    console.error('Error:', error);
    process.exit(1);
  }
}

main();
