FROM mono:latest
ARG UID=1000
ARG GID=1000
RUN groupadd -g "${GID}" mono &&\
    useradd -g mono -u "${UID}" mono &&\
    apt-get update -y &&\
    apt-get install -y wget unzip
USER mono
WORKDIR /home/mono
RUN wget "https://github.com/Ethorbit/FastDL-Generator/releases/download/1.0.11/Release.zip" &&\
    unzip ./Release.zip
ENTRYPOINT [ "mono", "/home/mono/FastDL Generator.exe" ]
