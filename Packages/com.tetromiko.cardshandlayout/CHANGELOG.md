# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-08-19

### Added
- Pure C# mathematical layout engine (`CardHandLayoutEngine`).
- Dynamic cascade compression based on container width.
- Neighbor space compensation algorithm for active hovered cards.
- Unity uGUI Card Hand Controller (`CardHandController`) with Drag & Drop reordering and ghost slot displacement.
- Card view component (`CardView`) with pointer interactions and spring motion smoothing (`Vector2.SmoothDamp`).
- Custom Editor Inspector with debug actions and Scene View Gizmos.
- Assembly definition files for Runtime and Editor.
- Sample demo script and scene structure (`Samples~/BasicDemo`).
