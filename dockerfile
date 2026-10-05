FROM dhi.io/dotnet:10-sdk-alpine
COPY Project /project
WORKDIR /project
RUN ["dotnet", "sln", "remove", "Hercules.Tests"]
RUN ["dotnet", "publish", "-o", "/app"]
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5000
ENTRYPOINT ["dotnet", "Hercules.Api.dll"]