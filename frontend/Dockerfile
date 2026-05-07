# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm install

# Copy source and build
COPY . .
# Set build environment variables if needed
ARG NUXT_PUBLIC_API_BASE
ENV NUXT_PUBLIC_API_BASE=$NUXT_PUBLIC_API_BASE

RUN npm run build

# Runtime stage
FROM node:20-alpine
WORKDIR /app

# Copy output from build stage
COPY --from=build /app/.output ./.output
COPY --from=build /app/package.json ./package.json

# Expose port 3000
ENV HOST=0.0.0.0
ENV PORT=3000
EXPOSE 3000

CMD ["node", ".output/server/index.mjs"]
