FROM mcr.microsoft.com/dotnet/sdk:7.0-cbl-mariner-amd64@sha256:acbe3a983072f5a0a475f1718b99f00718b0456e81836d2100c7637f6e0ba558 AS build

ARG TARGETARCH
ARG TARGETOS

RUN arch=$TARGETARCH \
    && if [ "$arch" = "amd64" ]; then arch="x64"; fi \
    && echo $TARGETOS-$arch > /tmp/rid

WORKDIR /source

COPY . .
RUN dotnet restore -r $(cat /tmp/rid) /p:PublishReadyToRun=true
RUN dotnet publish src/JamieMagee.Stethoscope.Cli/JamieMagee.Stethoscope.Cli.csproj -c Release -f net7.0 -o /app -r $(cat /tmp/rid) --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishSingleFile=true


FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:7.0-cbl-mariner-distroless@sha256:76c1c826687a3b6b7badbe8187c6e109ad73fe56aa0b7e8485c1dab74fe77d13
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./JamieMagee.Stethoscope.Cli"]
