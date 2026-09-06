## [Unreleased]

### Added
- Created agentic markdown files suite including root `AGENTS.md` and scoped instructions (`squad-draft/AGENTS.md`, `squad-api/AGENTS.md`, `squad-func/AGENTS.md`).
- Added specialist agent personas under `.agents/` (`code-reviewer.md`, `reactjs-specialist.md`, `dotnet-specialist.md`).
- Added Antigravity skills under `.agents/skills/` (`code-reviewer/SKILL.md`, `reactjs-specialist/SKILL.md`).
- Implemented frontend accessibility overhaul for `squad-draft` (WCAG 2.1 AA keyboard navigation, live region announcements, image error fallbacks, and accessible WAI-ARIA modal dialogs).


# [0.3.0](https://github.com/seanr89/SquadComplete/compare/v0.2.0...v0.3.0) (2026-03-24)


### Features

* disable leaderboard when draft is incomplete ([#20](https://github.com/seanr89/SquadComplete/issues/20)) ([eef88f3](https://github.com/seanr89/SquadComplete/commit/eef88f33932e367099793d0a38755bb656f227bb))



# [0.2.0](https://github.com/seanr89/SquadComplete/compare/v0.1.0...v0.2.0) (2026-03-22)


### Features

* add Makefile for project build, cleanup, and git branch trimming automation ([c79ece2](https://github.com/seanr89/SquadComplete/commit/c79ece2b0237912bcecae852eee2076440a8f98d))



# [0.1.0](https://github.com/seanr89/SquadComplete/compare/6ce9b5a5a441c2d68b23e1c0331b03b68d5cf237...v0.1.0) (2026-03-18)


### Bug Fixes

* configure dotnet build output path using PublishDir property and an environment variable. ([fe5adf4](https://github.com/seanr89/SquadComplete/commit/fe5adf441eb582e74fa222eecf9c5ebef50dfaf5))
* Correctly quote `mappedPosition` string in `player_fixture_statistics` SQL insert statement. ([3eb69f7](https://github.com/seanr89/SquadComplete/commit/3eb69f74b88f42e83f8fa9e192464a29231c37e9))
* ensure home and away team IDs are added to the unique team set. ([edcb002](https://github.com/seanr89/SquadComplete/commit/edcb002426e27492f4c74e47db1baac33289ed46))
* Removed extraneous slash from Azure Function build and package paths in the deployment workflow. ([ca591fa](https://github.com/seanr89/SquadComplete/commit/ca591fa9882797f643bf883d69a96f4bb5d218c1))
* Update Azure Function App deployment package path to `./output` and add a debug echo step for the package variable. ([a6d2f98](https://github.com/seanr89/SquadComplete/commit/a6d2f985910d0d02a179d52ba543db7d11c2474e))
* Updated Azure Function App deployment workflow to correctly resolve package paths. ([369d036](https://github.com/seanr89/SquadComplete/commit/369d03660ecf3e096c52e95f78321164be2b8ea8))


### Features

* Add `output.json` containing fixture data for players. ([d024d78](https://github.com/seanr89/SquadComplete/commit/d024d78cea5f368fddc8509594177dd9000ebb24))
* add about dialog with changelog and contact form ([#7](https://github.com/seanr89/SquadComplete/issues/7)) ([1f0820f](https://github.com/seanr89/SquadComplete/commit/1f0820f30cdb26c1a171bf8cf98a96d92aae78f5))
* Add Azure Static Web Apps CI/CD workflow and refactor player display to use a Pitch component for formation-based selection. ([e4f8da2](https://github.com/seanr89/SquadComplete/commit/e4f8da288d23ae07b3793d4796e52453d9134c18))
* Add C# Azure Function project and remove SQL schema files. ([575eed1](https://github.com/seanr89/SquadComplete/commit/575eed1ce122537e7d079b4b2c2010b309972f03))
* Add CI/CD workflow and initial application settings for squad-api. ([d98c172](https://github.com/seanr89/SquadComplete/commit/d98c172d6dccf7e2d065a3df1bb34fef52c9beeb))
* Add CORS configuration to the API and trigger the CI workflow on the `uiapisync` branch. ([55200b2](https://github.com/seanr89/SquadComplete/commit/55200b2e15eaa74ab9dfb67677a5e8c539663dd1))
* Add Daily Leaderboard UI screen with test data ([#13](https://github.com/seanr89/SquadComplete/issues/13)) ([e68dc8d](https://github.com/seanr89/SquadComplete/commit/e68dc8dd96a501ff648a471cc6df400fcbb2a3f2))
* Add DailyFixtures function and adjust FixtureStats timer schedule. ([8264ad4](https://github.com/seanr89/SquadComplete/commit/8264ad43c60e436f8b868b4cb955eec5269c1f27))
* Add database models and integrate Entity Framework Core with PostgreSQL for fixture and player data management, updating the SquadSelector function to use this new persistence layer and modifying its timer schedule. ([9fc6c67](https://github.com/seanr89/SquadComplete/commit/9fc6c67ebb4bcb8a7be3601e8e3ac0d3ba899886))
* add feedback submission support ([#10](https://github.com/seanr89/SquadComplete/issues/10)) ([42c46a2](https://github.com/seanr89/SquadComplete/commit/42c46a222ffa5f8ce7c56c8f5f154383da184a5a))
* Add functionality to retrieve and store fixture player statistics. ([9eff409](https://github.com/seanr89/SquadComplete/commit/9eff4096ee7e8f0f302ffe3470a1e0b8701864de))
* Add home and away goal counts to the Fixture model, database schema, and API endpoints, and refactor SquadSelector logging and constructor. ([37c5498](https://github.com/seanr89/SquadComplete/commit/37c54981e0d73910830bb29be29eb17db73ed036))
* Add player statistics data fixture and update database service and host configuration. ([3fd3db2](https://github.com/seanr89/SquadComplete/commit/3fd3db27c7b1d60f25aab86af23544104d6c77e2))
* Create feedback table with columns for user feedback and associated indexes. ([3b0381f](https://github.com/seanr89/SquadComplete/commit/3b0381ff120c9a70d95d0f025d48f5e246af8275))
* create initial database schema including tables for users, players, teams, fixtures, leagues, formations, game records, and user squads. ([876d31c](https://github.com/seanr89/SquadComplete/commit/876d31c9c450b34f0c3b6da1d0f753ac5ed66163))
* Disable remote build, SCM build, and Oryx build for Azure Function App deployment. ([19948aa](https://github.com/seanr89/SquadComplete/commit/19948aad5ae6fed9e632f8559485bdb6c7037025))
* Display player rating on cards, implement API error handling with UI, and add XML documentation to backend endpoints and services. ([ac6da8e](https://github.com/seanr89/SquadComplete/commit/ac6da8e3627e87388e5bde1a7befd5742316193e))
* Dynamically position players on the pitch based on their formation and clean up unused DTO properties. ([#11](https://github.com/seanr89/SquadComplete/issues/11)) ([b5225fb](https://github.com/seanr89/SquadComplete/commit/b5225fb6285f566f47c308ec43ec2ff9accad71e))
* Eagerly load and directly store Formation objects in GameRecords, and add new fixture and output JSON files. ([b9feb2b](https://github.com/seanr89/SquadComplete/commit/b9feb2b9de3a8ff5477048f73c38de0820f3f8f4))
* Enable path filtering and refine build output and package paths in the deploy workflow. ([016ccb0](https://github.com/seanr89/SquadComplete/commit/016ccb0b54133956cae94f394a573d3722a334a5))
* Exclude substitute players from game statistics and refactor player list rendering with a dedicated component. ([b670a48](https://github.com/seanr89/SquadComplete/commit/b670a48e64d45c6a4353c71f552afb5d203fdb64))
* Fetch daily squads from an API with local fallback, and add API service and environment configuration. ([de7e009](https://github.com/seanr89/SquadComplete/commit/de7e009fcadd915821660ad88166f278cff4c26c))
* Implement daily persistent storage for the draft state using local storage. ([8066bec](https://github.com/seanr89/SquadComplete/commit/8066becc452d2bf3bda70d39e7472193ceb8a8a1))
* Implement dynamic formation generation and fetch formation data from the API. ([#14](https://github.com/seanr89/SquadComplete/issues/14)) ([8fa4ca6](https://github.com/seanr89/SquadComplete/commit/8fa4ca6bd17006ac169b58bb1abd8760681bff0a))
* Implement fixture and league data retrieval, update function app dependencies and telemetry setup, and add a new `squad-draft` build workflow. ([b6b9dce](https://github.com/seanr89/SquadComplete/commit/b6b9dcedbc2e82769162c07020e1424ccc6eb8c0))
* Implement GameRecord CRUD endpoints, add corresponding HTTP client requests, and introduce a GitHub Actions workflow for Azure Function App deployment. ([a75b5f0](https://github.com/seanr89/SquadComplete/commit/a75b5f0801e6aa9f6d738ec63218e6bd4761f102))
* Implement player statistics processing and game record management, including new API services and DTOs. ([0b03931](https://github.com/seanr89/SquadComplete/commit/0b03931e9a9bddcb16bc8b2da16f4ab0f6b9ae02))
* Implement processing and storage for fixture player statistics, adding a sample output JSON. ([02a4b9e](https://github.com/seanr89/SquadComplete/commit/02a4b9e53b04410ab4db9729949acaa551792d06))
* Initialize squad-api with database models, context, and API endpoints for core entities. ([6ce9b5a](https://github.com/seanr89/SquadComplete/commit/6ce9b5a5a441c2d68b23e1c0331b03b68d5cf237))
* Introduce game record and tag models, database schema, and persistence for selected squads. ([69bc5ca](https://github.com/seanr89/SquadComplete/commit/69bc5ca82e19f8f2171efe8c56a5b0d2535ac93a))
* Introduce user and squad management with formation models and calculated team formations in game records. ([9a18f9a](https://github.com/seanr89/SquadComplete/commit/9a18f9a7e755a2276b8815c41ff311c94b1672e8))
* Introduce user, formation, and squad models with fixture goal tracking and minor UI adjustments. ([dc580bb](https://github.com/seanr89/SquadComplete/commit/dc580bb440c98479e03f05e57359051a30ab2db0))
* Re-enabled `GameRecordDto.Id`, removed several DTO mapping properties, and increased fixture processing limit from 5 to 8. ([840a08e](https://github.com/seanr89/SquadComplete/commit/840a08e87674a8b7a8c16463174fd909f4825498))
* Redesign PlayerCard UI by removing player rating, adding image backing, and updating text layout, along with a minor squad display text change. ([a9fc239](https://github.com/seanr89/SquadComplete/commit/a9fc2390cbff92eb309a6f428f0d0a9e22945044))
* skip processing stats for substitute players and ensure player ratings are non-negative ([dc3400c](https://github.com/seanr89/SquadComplete/commit/dc3400cb16aef4b3b87760fdf53e9c0815ea027a))
* Standardize player position mapping, extend fixture stats timer, and configure API CORS with conditional HTTPS redirection. ([bfc468d](https://github.com/seanr89/SquadComplete/commit/bfc468d3fed17c40be300bcda7ac7cbfd1fbaa92))
* **ui:** add team screenshot export functionality ([#15](https://github.com/seanr89/SquadComplete/issues/15)) ([e579bfc](https://github.com/seanr89/SquadComplete/commit/e579bfca6847f7c3f9677f6fa770114415a5fb27))
* Update daily fixture processing to be asynchronous, refine API service fixture retrieval, and add XML documentation to API endpoints. ([9a80be2](https://github.com/seanr89/SquadComplete/commit/9a80be25d1e9fa6a64243a0dbb2fafae71785846))
* update game creation error log message and include exception details in logging. ([706e758](https://github.com/seanr89/SquadComplete/commit/706e75815bb610be24cdc398deb9d24af947c176))



