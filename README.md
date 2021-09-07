# ErgoUtilsUploadService
An upload service used by [ErgoUtils](https://github.com/anon-real/ErgoUtils) to upload NFT files to [nft.storage](https://nft.storage/).

## API Endpoints
|Endpoint|Method|Desciption|
|---|:---:|---|
|`/ipfs`|`POST`|Store a file|
|`/ipfs/check/{cid}`|`GET`|Check if a CID of an NFT is being stored by nft.storage.
|`/version`|`GET`|Check service's commit hash|
