const p = require('path')
const { execSync } = require('child_process')
const { copyFileSync, existsSync, unlinkSync } = require('fs')
const { exit } = require('process')

const MIN_NODE_VERSION = 'v24.1.0'
const NODE_PATH = './node-base.exe'

// Get the command line argument
const exeName = process.argv[2]
if (!exeName) {
  console.error(
    '❌ Please provide the executable file name, e.g., node build-sea.js myapp'
  )
  exit(1)
}

// Get the Node.js executable path
let nodePath = NODE_PATH
if (!existsSync(nodePath)) {
  nodePath = process.execPath
  console.warn(
    `⚠️  ${p.basename(NODE_PATH)} not found in the root directory of the project, using node.exe of process.execPath instead`
  )
} else {
  console.log(`🔍 Found the node-base.exe in the root directory of the project`)
}

// Check the Node.js version
const gotVersion = execSync(`"${nodePath}" --version`).toString().trim()
console.log(`🔍 The Node.js version: ${gotVersion}`)
if (compareVersions(gotVersion, MIN_NODE_VERSION) < 0) {
  console.error(
    `❌ Node.js version must be ≥ ${MIN_NODE_VERSION}, but the current version is ${gotVersion}`
  )
  exit(1)
}

// Set the target executable file name
const targetExe = `dist/${exeName}.exe`

// Delete old exe and sea-prep.blob files (if they exist)
if (existsSync(targetExe)) {
  console.log(`🧹 Deleting old ${targetExe}...`)
  unlinkSync(targetExe)
}

if (existsSync('sea-prep.blob')) {
  console.log('🧹 Deleting old sea-prep.blob...')
  unlinkSync('sea-prep.blob')
}

// Copy the Node executable to the target exe file
console.log(`📦 Copying Node executable to ${targetExe}...`)
copyFileSync(nodePath, targetExe)

// Check if configuration files exist
if (!existsSync('sea-config.json')) {
  console.error('❌ Missing sea-config.json or sea-prep.blob files')
  exit(1)
}

// Apply the SEA configuration
console.log('🔧 Applying SEA configuration...')
execSync(`node --experimental-sea-config sea-config.json`, { stdio: 'inherit' })

// Inject the BLOB data
console.log('🧬 Injecting SEA blob data...')
execSync(
  `npx postject ${targetExe} NODE_SEA_BLOB dist/sea-prep.blob --sentinel-fuse NODE_SEA_FUSE_fce680ab2cc467b6e072b8b5df1996b2`,
  { stdio: 'inherit' }
)

console.log(`✅ Build completed: ${targetExe}`)

// --- Functions ---

function compareVersions(a, b) {
  const pa = a.replace(/^v/, '').split('.').map(Number)
  const pb = b.replace(/^v/, '').split('.').map(Number)
  for (let i = 0; i < Math.max(pa.length, pb.length); i++) {
    const na = pa[i] || 0
    const nb = pb[i] || 0
    if (na > nb) return 1
    if (na < nb) return -1
  }
  return 0
}