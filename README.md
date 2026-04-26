# protoc-gen-twincat
A protobuf compiler plugin for TwinCAT that generates POUs and DUTs from *.proto files.

# Usage

* Download the latest release for your system (Linux/Windows) from the Releases page and unzip.
* You can either put the plugin executable on your `PATH` or call it explicitly with `--plugin`.

Important: `protoc` does not use the OS PATH to locate `.proto` import files. Use -I / --proto_path to tell `protoc` where to find imported `.proto` files (for example the included tchaxx-extensions.proto).

Examples

1) If the plugin is on your `PATH` (recommended)
- Linux (make sure the binary is executable):
  ```bash
  export PATH="/path/to/protoc-gen-twincat:$PATH"
  export PATH="/path/to/include/tchaxx-extensions.proto:$PATH"
  protoc --twincat_out=./out example.proto
  ```
- Windows (PowerShell):
  ```powershell
  setx PATH "$env:PATH;C:\path\to\protoc-gen-twincat"
  setx PATH "$env:PATH;C:\path\to\include\tchaxx-extensions.proto"
  protoc --twincat_out=.\out example.proto
  ```

1) If you prefer to call the plugin executable directly with `-I` and `--plugin`
- Linux:
  ```bash
  protoc -Iproto -I/path/to/includes --plugin=protoc-gen-twincat=/full/path/to/protoc-gen-twincat --twincat_out=./out example.proto
  ```
- Windows:
  ```powershell
  protoc -I proto -I C:\path\to\includes --plugin=protoc-gen-twincat=C:\full\path\to\protoc-gen-twincat.exe --twincat_out=.\out example.proto
  ```

Notes
- The `protoc` plugin name is derived from the flag: `--twincat_out` will cause `protoc` to invoke an executable named `protoc-gen-twincat` (or the path you provide via `--plugin`).
- Ensure the directory that contains `tchaxx-extensions.proto` is included via `-I` or `--proto_path`, so imports like:
  `import "tchaxx-extensions.proto";`
  are resolved by `protoc`.
- If you see "executable not found" errors, either add the plugin to `PATH` or use the `--plugin` flag with the full path. On Linux make it executable (`chmod +x`).

# Using `tchaxx-extensions.proto`

The plugin is driven by custom protobuf options defined in [`proto/tchaxx-extensions.proto`](proto/tchaxx-extensions.proto). Import it from any `.proto` you want to compile, and make sure its directory is on `protoc`'s `-I` / `--proto_path`:

```proto
syntax = "proto3";
import "tchaxx-extensions.proto";

package my.package;
```

All options live under the `tchaxx.*` namespace.

## Prefixes

Prefixes control the **names of generated TwinCAT artifacts** (DUTs, POUs, enums) and the **property name** the POU exposes for its underlying ST instance. They exist solely to avoid name clashes — most importantly when the same protobuf message is emitted as both a DUT and a function block (POU) — and never affect field names, enum values, or any wire-level/protobuf semantics.

All prefix options take a `Prefix` message with two fields:

- `type` — prepended to the **generated type/file name** (e.g. `type: "ST_"` on message `Foo` produces `ST_Foo.TcDUT`).
- `property` — prepended to the **public property name** that the generated POU exposes for accessing the underlying ST instance. Only meaningful for `st_prefix` / `global_st_prefix`. If omitted, the property name falls back to the bare message name (`Foo`).

Prefixes can be set at three levels — **file (global)**, **message**, and **enum** — with the more specific level overriding the broader one. If nothing is set at any level, the plugin falls back to built-in defaults: `ST_` for structures, `FB_` for function blocks, `E_` for enums.

### 1. File level (global) — applies to every type in the file

Use `global_*_prefix` to set a default for every type generated from a single `.proto` file.

| Option | Applies to | Example |
| --- | --- | --- |
| `tchaxx.global_st_prefix` | structures (DUTs) | `option (tchaxx.global_st_prefix) = {type: "ST_", property: "st"};` |
| `tchaxx.global_fb_prefix` | function blocks (POUs) | `option (tchaxx.global_fb_prefix) = {type: "FB_"};` |
| `tchaxx.global_enum_prefix` | enumerations | `option (tchaxx.global_enum_prefix) = {type: "E_"};` |

```proto
option (tchaxx.global_st_prefix)   = {type: "ST_", property: "st"};
option (tchaxx.global_fb_prefix)   = {type: "FB_"};
option (tchaxx.global_enum_prefix) = {type: "E_"};
```

For a message `Foo` that is generated as both a DUT and a POU, the output looks like:

```iecst
FUNCTION_BLOCK FB_Foo
VAR
    _stFoo : ST_Foo;                     // backing instance (uses internal "_st" prefix)
END_VAR

PROPERTY PUBLIC stFoo : ST_Foo           // <-- "property" prefix → "st" + "Foo"
```

### 2. Message level — overrides the file-level prefix for one message

Set `st_prefix` and/or `fb_prefix` on an individual message to override the file default for that message only.

| Option | Effect |
| --- | --- |
| `tchaxx.st_prefix` | Prefix for the generated DUT name (and ST property name, via `property`). |
| `tchaxx.fb_prefix` | Prefix for the generated POU name. |

```proto
message Device {
  option (tchaxx.st_prefix) = {type: "ST_DEV_", property: "stDev"}; // → ST_DEV_Device, property "stDevDevice"
  option (tchaxx.fb_prefix) = {type: "FB_DEV_"};                    // → FB_DEV_Device
}
```

### 3. Enum level — overrides the file-level enum prefix for one enum

| Option | Effect |
| --- | --- |
| `tchaxx.enum_prefix` | Prefix for the generated enum type name. |

```proto
enum Status {
  option (tchaxx.enum_prefix) = {type: "E_STATUS_"}; // → E_STATUS_Status
  STATUS_IDLE    = 0;
  STATUS_RUNNING = 1;
  STATUS_ERROR   = 2;
}
```

### Resolution order

For each generated artifact, the plugin picks the first prefix it finds, walking from most-specific to least-specific:

1. Message-level (`st_prefix` / `fb_prefix`) or enum-level (`enum_prefix`) option on the type itself.
2. File-level (`global_st_prefix` / `global_fb_prefix` / `global_enum_prefix`).
3. Built-in default (`ST_` / `FB_` / `E_`).

## Message options

```proto
message Device {
  option (tchaxx.attribute_pack_mode) = EPM_ONE_BYTE_ALIGNED; // {attribute 'pack_mode' := '1'}
}
```

| Option | Effect |
| --- | --- |
| `tchaxx.attribute_pack_mode` | Adds `{attribute 'pack_mode' := 'N'}`. Values: `EPM_ONE_BYTE_ALIGNED`, `EPM_TWO_BYTES_ALIGNED`, `EPM_FOUR_BYTES_ALIGNED`, `EPM_EIGHT_BYTES_ALIGNED` — see [Beckhoff InfoSys: pack_mode](https://infosys.beckhoff.com/content/1033/tc3_plc_intro/2529746059.html). |

> Message-level prefix options (`st_prefix`, `fb_prefix`) are documented in [Prefixes](#prefixes).

## Field options

```proto
message ScalarTypes {
  string  name        = 1 [(tchaxx.max_string_len) = 256];
  uint32  id          = 2 [(tchaxx.integer_type) = EIT_DWORD];
  int32   temperature = 3 [(tchaxx.integer_type) = EIT_DINT,
                            (tchaxx.attribute_display_mode) = EDM_HEX];
  repeated uint32 codes = 4 [(tchaxx.array_len) = 10,
                              (tchaxx.integer_type) = EIT_WORD]; // ARRAY[0..9] OF WORD
  bytes   blob        = 5 [(tchaxx.array_len) = 64];             // ARRAY[0..63] OF BYTE
  int64   timestamp   = 6 [(tchaxx.custom_type) = "LTIME"];
}
```

| Option | Effect |
| --- | --- |
| `tchaxx.array_len` | Generates `ARRAY[0..N-1] OF ...` for `repeated`/`bytes` fields. |
| `tchaxx.integer_type` | Maps the protobuf integer to a TwinCAT type: `EIT_BYTE`, `EIT_WORD`, `EIT_DWORD`, `EIT_LWORD`, `EIT_SINT`, `EIT_USINT`, `EIT_INT`, `EIT_UINT`, `EIT_DINT`, `EIT_UDINT`, `EIT_LINT`, `EIT_ULINT`. |
| `tchaxx.max_string_len` | Generates `STRING(N)` for string fields. |
| `tchaxx.attribute_display_mode` | Adds `{attribute 'displaymode':='dec'\|'hex'\|'bin'}` — see [Beckhoff InfoSys: displaymode](https://infosys.beckhoff.com/content/1033/tc3_plc_intro/2529633163.html). |
| `tchaxx.custom_type` | Replaces the generated TwinCAT type with a verbatim string (e.g. `"LTIME"`, `"T_MaxString"`). |

## Enum options

```proto
enum Status {
  option (tchaxx.enum_integer_type) = EIT_DWORD;
  STATUS_IDLE    = 0;
  STATUS_RUNNING = 1;
  STATUS_ERROR   = 2;
}
```

| Option | Effect |
| --- | --- |
| `tchaxx.enum_integer_type` | Underlying integer type of the enum (same `EIT_*` set as `integer_type`). |

> Enum-level prefix option (`enum_prefix`) is documented in [Prefixes](#prefixes).

## Option enum reference

The option values used above are themselves protobuf enums declared in [`proto/tchaxx-extensions.proto`](proto/tchaxx-extensions.proto). Use the symbolic names — not the integers — when setting options.

### `EnumPackMode` — used by `tchaxx.attribute_pack_mode`

Maps to TwinCAT's [`{attribute 'pack_mode'}`](https://infosys.beckhoff.com/content/1033/tc3_plc_intro/2529746059.html).

| Value | Generated attribute |
| --- | --- |
| `EPM_DEFAULT` | *(no attribute emitted)* |
| `EPM_ONE_BYTE_ALIGNED` | `{attribute 'pack_mode' := '1'}` |
| `EPM_TWO_BYTES_ALIGNED` | `{attribute 'pack_mode' := '2'}` |
| `EPM_FOUR_BYTES_ALIGNED` | `{attribute 'pack_mode' := '4'}` |
| `EPM_EIGHT_BYTES_ALIGNED` | `{attribute 'pack_mode' := '8'}` |

### `EnumDisplayMode` — used by `tchaxx.attribute_display_mode`

Maps to TwinCAT's [`{attribute 'displaymode'}`](https://infosys.beckhoff.com/content/1033/tc3_plc_intro/2529633163.html).

| Value | Generated attribute |
| --- | --- |
| `EDM_DEFAULT` | *(no attribute emitted)* |
| `EDM_DEC` | `{attribute 'displaymode':='dec'}` |
| `EDM_HEX` | `{attribute 'displaymode':='hex'}` |
| `EDM_BIN` | `{attribute 'displaymode':='bin'}` |

### `EnumIntegerTypes` — used by `tchaxx.integer_type` and `tchaxx.enum_integer_type`

Selects the [TwinCAT integer data type](https://infosys.beckhoff.com/content/1033/tc3_plc_intro/2529399691.html) used for a field or enum's underlying representation. `EIT_DEFAULT` keeps the natural mapping derived from the protobuf scalar type.

| Value | TwinCAT type | Width |
| --- | --- | --- |
| `EIT_DEFAULT` | *(default mapping)* | — |
| `EIT_BYTE`  | `BYTE`  | 8-bit unsigned |
| `EIT_WORD`  | `WORD`  | 16-bit unsigned |
| `EIT_DWORD` | `DWORD` | 32-bit unsigned |
| `EIT_LWORD` | `LWORD` | 64-bit unsigned |
| `EIT_SINT`  | `SINT`  | 8-bit signed |
| `EIT_USINT` | `USINT` | 8-bit unsigned |
| `EIT_INT`   | `INT`   | 16-bit signed |
| `EIT_UINT`  | `UINT`  | 16-bit unsigned |
| `EIT_DINT`  | `DINT`  | 32-bit signed |
| `EIT_UDINT` | `UDINT` | 32-bit unsigned |
| `EIT_LINT`  | `LINT`  | 64-bit signed |
| `EIT_ULINT` | `ULINT` | 64-bit unsigned |

# Develop

## Step 1: Install Protocol Buffers Compiler (protoc)
1. Download the latest version of the Protocol Buffers compiler (protoc) from the [official release page](https://github.com/protocolbuffers/protobuf/releases).
2. Extract the downloaded archive to a directory of your choice.
3. Add the `bin` directory of the extracted archive to your system's PATH environment variable.

## Step 2: Clone the Repository
```sh
git clone https://github.com/TcHaxx/protoc-gen-twincat.git
cd protoc-gen-twincat
```

## Step 3: Build the Plugin
1. Ensure you have a `dotnet` environment set up on your machine.
2. Run the following command to build the plugin:
```sh
dotnet build -c Release 
```

## Step 4: Generate TwinCAT Files
1. Create or obtain your `.proto` files.
2. `CD` into `proto/` directory 
3. Create the output directory, e.g. `.out/`
   ```sh
   mkdir .out
   ```
4. Run the following command to generate TwinCAT files:
```sh
protoc --plugin=protoc-gen-twincat=../src/protoc-gen-twincat/bin/Release/net10.0/protoc-gen-twincat.exe --twincat_out ../.out example.proto
```
This will generate the corresponding `TcPOU` and `TcDUT` files in the current directory.

# Resources

* [protobuf - Language Guide (proto 3)](https://protobuf.dev/programming-guides/proto3/)