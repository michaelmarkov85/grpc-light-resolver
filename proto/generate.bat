protoc.exe --proto_path=. --grpc_out=../grpcServer  --grpc_out=../grpcClient --csharp_out=../grpcServer  --csharp_out=../grpcClient api.proto  --plugin=protoc-gen-grpc=grpc_csharp_plugin.exe