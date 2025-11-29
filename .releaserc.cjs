module.exports = {
  branches: [
    "main",
    { name: "develop", prerelease: true },
  ],
  tagFormat: "v${version}",
  plugins: [
    [
      "@semantic-release/commit-analyzer",
      {
        preset: "conventionalcommits",
        parserOpts: {
          headerPattern: /^(\w*)(?:\((.*)\))?: (.*)$/,
          headerCorrespondence: ["type", "scope", "subject"],
        },
      },
    ],
    "@semantic-release/release-notes-generator",
    [
      "@semantic-release/github",
      {
        releaseNameTemplate: "Release v${nextRelease.version}",
        assets: [],
      },
    ],
]};
