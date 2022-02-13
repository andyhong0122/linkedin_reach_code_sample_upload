# linkedin_reach_code_sample_upload
This repository deals with file uploads to the AWS S3 bucket.

A user may upload multiple files at once, and the file contents are read and then sent to the S3 storage and saved as objects.

Upon successful upload, the guid and file url are stored as Dictionary, and stored in the database.

When a user logs in, they may request their files from the database.
