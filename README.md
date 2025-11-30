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