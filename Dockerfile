FROM mcr.microsoft.com/dotnet/sdk:7.0-cbl-mariner-amd64@sha256:646176e541f376c3eec408c281428007371d43b7f57dea8015391d6ec1c94900 AS build

ARG TARGETARCH
ARG TARGETOS

RUN arch=$TARGETARCH \
    && if [ "$arch" = "amd64" ]; then arch="x64"; fi \
    && echo $TARGETOS-$arch > /tmp/rid

WORKDIR /source

COPY . .
RUN dotnet restore -r $(cat /tmp/rid) /p:PublishReadyToRun=true
RUN dotnet publish src/JamieMagee.Stethoscope.Cli/JamieMagee.Stethoscope.Cli.csproj -c Release -f net7.0 -o /app -r $(cat /tmp/rid) --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishSingleFile=true


FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:7.0-cbl-mariner-distroless@sha256:a67275ebd532d5a002f7346be1974700175fd4e0c30c5ac363a4c666676d5cfd
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./JamieMagee.Stethoscope.Cli"]
