#!/usr/bin/env tsx

/**
 * ID Validation Script
 *
 * Extracts all MongoDB IDs from TypeScript asset files and maps them to
 * ItemTpl constants from SPT's server-csharp repository.
 *
 * Usage: npx tsx scripts/validate-ids.ts
 * Output: scripts/id-mapping.csv
 */

import fs from 'fs';
import path from 'path';

// MongoDB ID pattern: 24 character hexadecimal string
const MONGO_ID_REGEX = /["']([a-f0-9]{24})["']/gi;

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
  const url = `https://raw.githubusercontent.com/sp-tarkov/server-csharp/main/Libraries/SPTarkov.Server.Core/Models/Enums/${fileName}`;

  console.log(`Fetching ${fileName} from GitHub...`);
  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`Failed to fetch ${fileName}: ${response.statusText}`);
  }

  const content = await response.text();
  const map = new Map<string, string>();

  // Parse lines like: public static readonly MongoId AMMO_127X108_B32 = new MongoId("5cde8864d7f00c0010373be1");
  const enumRegex = /public static readonly MongoId (\w+) = new MongoId\("([a-f0-9]{24})"\);/g;
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
 * Query SPT database API for item name
 */
async function getItemName(id: string): Promise<string> {
  const url = `https://db.sp-tarkov.com/api/item?id=${id}&locale=en`;

  try {
    const response = await fetch(url);

    if (!response.ok) {
      return 'UNKNOWN';
    }

    const data = await response.json();
    return data?.name || data?._props?.Name || 'UNKNOWN';
  } catch (error) {
    console.warn(`Failed to fetch name for ${id}: ${error}`);
    return 'UNKNOWN';
  }
}

/**
 * Build complete mapping (skip name lookup for now - API not reliable)
 */
async function buildMapping(ids: Set<string>, enumMap: Map<string, string>): Promise<IdMapping[]> {
  const mappings: IdMapping[] = [];

  for (const id of ids) {
    const constant = enumMap.get(id) || 'NOT_FOUND';
    mappings.push({ tsId: id, name: '', constant });
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
    .map(m => `${m.tsId},,${m.constant}`)
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
    const mappings = await buildMapping(ids, enumMap);

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
