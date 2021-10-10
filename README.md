# Mirrors.NET

Mirrors.NET is a simple, fast, and reliable mirroring service build with .NET. It's currently serving for [ZJU's mirror site](http://mirrors.zju.edu.cn).

## Architecture

### Overall

### Manager

#### API

ref: https://github.com/mirrorz-org/mirrorz#data-format-v15-draft

#### Configs

Example:

```text
Configs
├── Packages
│   └── debian.yml
├── Releases
│   └── debian-iso.yml
└── site.yml
```

### Worker

