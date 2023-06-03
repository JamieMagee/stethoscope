FROM mcr.microsoft.com/dotnet/sdk:7.0-cbl-mariner-amd64@sha256:4ee38df202402ee186b0d38fb3efb99449c098831ca2a63977e656ab99ed580a AS build

ARG TARGETARCH
ARG TARGETOS

RUN arch=$TARGETARCH \
    && if [ "$arch" = "amd64" ]; then arch="x64"; fi \
    && echo $TARGETOS-$arch > /tmp/rid

WORKDIR /source

COPY . .
RUN dotnet restore -r $(cat /tmp/rid) /p:PublishReadyToRun=true
RUN dotnet publish src/JamieMagee.Stethoscope.Cli/JamieMagee.Stethoscope.Cli.csproj -c Release -f net7.0 -o /app -r $(cat /tmp/rid) --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishSingleFile=true


FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:7.0-cbl-mariner-distroless@sha256:f0f2b603f93671ef2705ee0f81d18decf9f55c467087cef6ee450d23e5911097
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./JamieMagee.Stethoscope.Cli"]
