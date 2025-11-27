# MatchZy Demo Upload API Endpoint Specification

This document describes exactly how MatchZy uploads demo files to your API endpoint. Use this specification to implement the receiving endpoint in your Express.js server.

## Overview

MatchZy uploads demo files as **HTTP POST requests** with the demo file as the request body. Each map in a match series generates a separate demo file that is uploaded individually.

## HTTP Request Details

### Method

```
POST
```

### Content Type

```
Content-Type: application/octet-stream
```

### Request Body

- **Format**: Raw binary data (the `.dem` file contents)
- **Size**: Variable (typically 10-200 MB, can be larger for long matches)
- **No encoding**: The file is sent as-is, not base64 encoded, not compressed/zipped

### HTTP Headers Sent by MatchZy

MatchZy sends the following headers with each upload request:

#### Required Headers (Always Present)

```
Content-Type: application/octet-stream
MatchZy-FileName: {demo_filename}.dem
MatchZy-MatchId: {match_id}
MatchZy-MapNumber: {map_number}
MatchZy-RoundNumber: {total_rounds}
```

#### Get5 Compatibility Headers (Also Always Present)

```
Get5-FileName: {demo_filename}.dem
Get5-MatchId: {match_id}
Get5-MapNumber: {map_number}
Get5-RoundNumber: {total_rounds}
```

#### Optional Custom Headers (If Configured)

If `matchzy_demo_upload_header_key` and `matchzy_demo_upload_header_value` are set, they are added to the request:

```
{header_key}: {header_value}
```

### Header Values Explained

| Header                | Type             | Description                               | Example                                            |
| --------------------- | ---------------- | ----------------------------------------- | -------------------------------------------------- |
| `MatchZy-FileName`    | String           | The filename of the demo file             | `1704067200_12345_de_dust2_TeamA_vs_TeamB.dem`     |
| `MatchZy-MatchId`     | String (numeric) | Unique identifier for the match           | `12345`                                            |
| `MatchZy-MapNumber`   | String (numeric) | Zero-indexed map number in the series     | `0` (first map), `1` (second map), `2` (third map) |
| `MatchZy-RoundNumber` | String (numeric) | Total number of rounds played in this map | `24`, `30`, etc.                                   |

**Important Notes:**

- `MapNumber` is **zero-indexed**: First map = `0`, second map = `1`, etc.
- For a BO3 match, you'll receive 3 separate uploads with `MapNumber` values: `0`, `1`, `2`
- `MatchId` is the same for all maps in a series
- `RoundNumber` is the total rounds (e.g., 16-14 = 30 rounds)

## Upload Timing

1. **When**: Upload starts **15 seconds** after the map ends and demo recording stops
2. **Frequency**: One upload per map (not per match)
3. **Asynchronous**: Upload happens in background, doesn't block the server

## Example Express.js Endpoint Implementation

Here's a complete example of how to receive demo uploads in Express.js:

```javascript
const express = require("express");
const fs = require("fs");
const path = require("path");

const app = express();

// IMPORTANT: Use express.raw() middleware to handle binary data
// Set limit to handle large files (e.g., 500MB)
app.post(
  "/api/demos/upload",
  express.raw({
    type: "application/octet-stream",
    limit: "500mb",
  }),
  async (req, res) => {
    try {
      // Extract metadata from headers
      const filename =
        req.headers["matchzy-filename"] || req.headers["get5-filename"];
      const matchId =
        req.headers["matchzy-matchid"] || req.headers["get5-matchid"];
      const mapNumber =
        req.headers["matchzy-mapnumber"] || req.headers["get5-mapnumber"];
      const roundNumber =
        req.headers["matchzy-roundnumber"] || req.headers["get5-roundnumber"];

      // Validate required headers
      if (!filename || !matchId || mapNumber === undefined) {
        return res.status(400).json({
          error: "Missing required headers",
          required: [
            "MatchZy-FileName",
            "MatchZy-MatchId",
            "MatchZy-MapNumber",
          ],
        });
      }

      // Log the upload
      console.log(
        `[Demo Upload] Receiving demo for match ${matchId}, map ${mapNumber}`
      );
      console.log(
        `[Demo Upload] Filename: ${filename}, Rounds: ${roundNumber}`
      );
      console.log(
        `[Demo Upload] File size: ${req.body.length} bytes (${(
          req.body.length /
          1024 /
          1024
        ).toFixed(2)} MB)`
      );

      // Create directory structure: demos/{matchId}/
      const matchDir = path.join(__dirname, "demos", matchId.toString());
      if (!fs.existsSync(matchDir)) {
        fs.mkdirSync(matchDir, { recursive: true });
      }

      // Save the demo file
      const filePath = path.join(matchDir, filename);
      fs.writeFileSync(filePath, req.body);

      console.log(`[Demo Upload] Demo saved: ${filePath}`);

      // Return success response
      res.status(200).json({
        success: true,
        message: "Demo uploaded successfully",
        matchId: matchId,
        mapNumber: parseInt(mapNumber),
        filename: filename,
        fileSize: req.body.length,
        savedPath: filePath,
      });
    } catch (error) {
      console.error("[Demo Upload] Error:", error);
      res.status(500).json({
        error: "Failed to save demo file",
        message: error.message,
      });
    }
  }
);

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`Demo upload server listening on port ${PORT}`);
});
```

## Important Implementation Notes

### 1. Body Parser Configuration

**CRITICAL**: You must use `express.raw()` with the correct content type. Do NOT use:

- ❌ `express.json()` - This will corrupt binary data
- ❌ `express.text()` - This will corrupt binary data
- ❌ `body-parser` with default settings - This will corrupt binary data

**CORRECT**:

```javascript
express.raw({ type: "application/octet-stream", limit: "500mb" });
```

### 2. File Size Limits

- Set appropriate limits (e.g., `500mb`) to handle large demo files
- Demos can be 10-200+ MB depending on match length
- Consider your server's memory and disk space

### 3. Header Case Sensitivity

HTTP headers are case-insensitive, but Express normalizes them to lowercase. Use:

```javascript
req.headers["matchzy-filename"]; // lowercase
// NOT req.headers['MatchZy-FileName']
```

### 4. Custom Headers (Authentication)

If you configured custom headers in MatchZy:

```javascript
const authToken = req.headers["authorization"]; // or whatever header key you set
if (authToken !== "Bearer your-expected-token") {
  return res.status(401).json({ error: "Unauthorized" });
}
```

### 5. Error Handling

MatchZy expects:

- **200-299 status codes**: Treated as success
- **400+ status codes**: Treated as failure, error logged

If your endpoint returns an error, MatchZy will:

- Log the error to server console
- Display error message in game chat
- Keep the demo file locally (it's always saved locally regardless of upload status)

### 6. Response Body

MatchZy reads the response body for logging, but doesn't require any specific format. However, returning JSON with useful information is recommended for debugging.

## Complete Example with Error Handling and Authentication

```javascript
const express = require("express");
const fs = require("fs");
const path = require("path");

const app = express();

// Middleware to parse binary data
app.post(
  "/api/demos/upload",
  express.raw({ type: "application/octet-stream", limit: "500mb" }),
  async (req, res) => {
    try {
      // Optional: Check authentication header
      const authHeader = req.headers["authorization"];
      if (authHeader !== "Bearer your-secret-token") {
        return res.status(401).json({ error: "Unauthorized" });
      }

      // Extract metadata
      const filename = req.headers["matchzy-filename"];
      const matchId = req.headers["matchzy-matchid"];
      const mapNumber = req.headers["matchzy-mapnumber"];
      const roundNumber = req.headers["matchzy-roundnumber"];

      // Validate
      if (!filename || !matchId || mapNumber === undefined) {
        return res.status(400).json({
          error: "Missing required headers",
          received: {
            filename: filename || "missing",
            matchId: matchId || "missing",
            mapNumber: mapNumber || "missing",
          },
        });
      }

      // Validate file size (optional)
      const fileSizeMB = req.body.length / 1024 / 1024;
      if (fileSizeMB > 500) {
        return res.status(413).json({ error: "File too large" });
      }

      // Create directory
      const matchDir = path.join(__dirname, "demos", matchId);
      fs.mkdirSync(matchDir, { recursive: true });

      // Save file
      const filePath = path.join(matchDir, filename);
      fs.writeFileSync(filePath, req.body);

      // Log success
      console.log(
        `✓ Demo uploaded: ${filename} (${fileSizeMB.toFixed(
          2
        )} MB) for match ${matchId}, map ${mapNumber}`
      );

      // Return success
      res.status(200).json({
        success: true,
        matchId: parseInt(matchId),
        mapNumber: parseInt(mapNumber),
        filename: filename,
        fileSize: req.body.length,
        savedAt: filePath,
      });
    } catch (error) {
      console.error("✗ Demo upload error:", error);
      res.status(500).json({
        error: "Internal server error",
        message: error.message,
      });
    }
  }
);

app.listen(3000, () => {
  console.log("Demo upload endpoint ready on port 3000");
});
```

## Testing Your Endpoint

You can test your endpoint using curl:

```bash
# Create a test demo file
echo "test demo content" > test.dem

# Upload it with required headers
curl -X POST http://localhost:3000/api/demos/upload \
  -H "Content-Type: application/octet-stream" \
  -H "MatchZy-FileName: test.dem" \
  -H "MatchZy-MatchId: 99999" \
  -H "MatchZy-MapNumber: 0" \
  -H "MatchZy-RoundNumber: 24" \
  --data-binary "@test.dem"
```

## Summary

- **Method**: POST
- **Content-Type**: `application/octet-stream`
- **Body**: Raw binary file data (not encoded, not compressed)
- **Headers**: `MatchZy-FileName`, `MatchZy-MatchId`, `MatchZy-MapNumber`, `MatchZy-RoundNumber`
- **Response**: HTTP 200-299 for success, 400+ for errors
- **Use**: `express.raw({ type: 'application/octet-stream', limit: '500mb' })` middleware
- **Frequency**: One upload per map (BO3 = 3 uploads, BO5 = 5 uploads)

## Common Pitfalls

1. **Using wrong body parser**: Must use `express.raw()`, not `express.json()`
2. **Case sensitivity**: Headers are lowercase in Express (`matchzy-filename`, not `MatchZy-FileName`)
3. **File size limits**: Set appropriate limits (500MB+ recommended)
4. **Missing headers validation**: Always check for required headers
5. **Not handling errors**: Return proper HTTP status codes
