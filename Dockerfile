FROM mcr.microsoft.com/dotnet/sdk:7.0-cbl-mariner-amd64@sha256:6524e086d667b723fb1b7926f7e6dff4e2d99c8108942a26e138775f57a840f3 AS build

ARG TARGETARCH
ARG TARGETOS

RUN arch=$TARGETARCH \
    && if [ "$arch" = "amd64" ]; then arch="x64"; fi \
    && echo $TARGETOS-$arch > /tmp/rid

WORKDIR /source

COPY . .
RUN dotnet restore -r $(cat /tmp/rid) /p:PublishReadyToRun=true
RUN dotnet publish src/JamieMagee.Stethoscope.Cli/JamieMagee.Stethoscope.Cli.csproj -c Release -f net7.0 -o /app -r $(cat /tmp/rid) --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishSingleFile=true


FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:7.0-cbl-mariner-distroless@sha256:cff2d129589a81cc8f8b0645cd6478b27dc227180801a37f9de62502423671ce
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./JamieMagee.Stethoscope.Cli"]
