FROM mcr.microsoft.com/dotnet/sdk:7.0-cbl-mariner-amd64@sha256:6ba72f0f6bd99ce19b705c965ef124af7e54c65ecefd37c14177a0ff2fc26e2e AS build

ARG TARGETARCH
ARG TARGETOS

RUN arch=$TARGETARCH \
    && if [ "$arch" = "amd64" ]; then arch="x64"; fi \
    && echo $TARGETOS-$arch > /tmp/rid

WORKDIR /source

COPY . .
RUN dotnet restore -r $(cat /tmp/rid) /p:PublishReadyToRun=true
RUN dotnet publish src/JamieMagee.Stethoscope.Cli/JamieMagee.Stethoscope.Cli.csproj -c Release -f net7.0 -o /app -r $(cat /tmp/rid) --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishSingleFile=true


FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:7.0-cbl-mariner-distroless@sha256:a2bf82ccdaa45ec1e44fce4bdde0b05e5286483735d168cfb7825cd0b3c09844
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./JamieMagee.Stethoscope.Cli"]
