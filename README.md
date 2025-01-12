# protoc-gen-twincat
A protobuf compiler plugin for TwinCAT that generates POUs and DUTs from *.proto files.

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
protoc --plugin=protoc-gen-twincat=../src/protoc-gen-twincat/bin/Release/net9.0/protoc-gen-twincat.exe --twincat_out ../.out example.proto
```
This will generate the corresponding `TcPOU` and `TcDUT` files in the current directory.

# Resources

* [protobuf - Language Guide (proto 3)](https://protobuf.dev/programming-guides/proto3/)