
const {
  compilerOptions: {paths = {}},
} = require('./tsconfig.json');

module.exports = {
  preset: 'jest-preset-angular',
  moduleNameMapper: {
    '^flat$': '<rootDir>/node_modules/flat/index.js'
  },
  setupFilesAfterEnv: ['<rootDir>/src/main/webapp/test.ts'],
  transform: {
    '^.+\\.js$': 'babel-jest'
  },
  testMatch: ['<rootDir>/src/main/webapp/app/**/@(*.)@(spec.ts)'],
};


