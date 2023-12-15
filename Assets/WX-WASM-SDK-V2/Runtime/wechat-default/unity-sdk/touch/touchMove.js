import { formatTouchEvent, numberToUint8Array } from '../utils';
const OnTouchMoveList = [];
function serializeTouch(touch) {
    const clientXByteArray = numberToUint8Array(touch.clientX, Float32Array);
    const clientYByteArray = numberToUint8Array(touch.clientY, Float32Array);
    const forceByteArray = numberToUint8Array(touch.force);
    const identifierByteArray = numberToUint8Array(touch.identifier, Uint32Array);
    const pageXByteArray = numberToUint8Array(touch.pageX, Float32Array);
    const pageYByteArray = numberToUint8Array(touch.pageY, Float32Array);
    const byteArray = new Uint8Array(clientXByteArray.length + clientYByteArray.length + forceByteArray.length + identifierByteArray.length + pageXByteArray.length + pageYByteArray.length);
    let offset = 0;
    byteArray.set(clientXByteArray, offset);
    offset += clientXByteArray.length;
    byteArray.set(clientYByteArray, offset);
    offset += clientYByteArray.length;
    byteArray.set(forceByteArray, offset);
    offset += forceByteArray.length;
    byteArray.set(identifierByteArray, offset);
    offset += identifierByteArray.length;
    byteArray.set(pageXByteArray, offset);
    offset += pageXByteArray.length;
    byteArray.set(pageYByteArray, offset);
    return byteArray;
}
function serializeTouches(touches) {
    const serializedTouches = touches.map(serializeTouch);
    const totalLength = serializedTouches.reduce((sum, touchByteArray) => sum + touchByteArray.length, 0);
    const byteArray = new Uint8Array(totalLength);
    let offset = 0;
    serializedTouches.forEach((touchByteArray) => {
        byteArray.set(touchByteArray, offset);
        offset += touchByteArray.length;
    });
    return byteArray;
}
function serializeOnTouchStartListenerResult(result) {
    const touchesByteArray = serializeTouches(result.touches);
    const changedTouchesByteArray = serializeTouches(result.changedTouches);
    const timeStampByteArray = numberToUint8Array(result.timeStamp, Uint32Array);
    const byteArray = new Uint8Array(touchesByteArray.length + changedTouchesByteArray.length + timeStampByteArray.length);
    let offset = 0;
    byteArray.set(touchesByteArray, offset);
    offset += touchesByteArray.length;
    byteArray.set(changedTouchesByteArray, offset);
    offset += changedTouchesByteArray.length;
    byteArray.set(timeStampByteArray, offset);
    return byteArray;
}
function WX_OnTouchMove() {
    const callback = (res) => {
        res.touches = res.touches.map((v) => formatTouchEvent(v));
        res.changedTouches = res.changedTouches.map((v) => formatTouchEvent(v));
        res.timeStamp = parseInt(res.timeStamp.toString(), 10);
        const serializedData = serializeOnTouchStartListenerResult(res);
        const buffer = GameGlobal.Module._malloc(serializedData.length);
        GameGlobal.Module.HEAPU8.set(serializedData, buffer);
        GameGlobal.Module.dynCall('viiii', GameGlobal.Module.WXTouchManager.onTouchMove, [buffer, serializedData.length, res.touches.length, res.changedTouches.length]);
        GameGlobal.Module._free(buffer);
    };
    OnTouchMoveList.push(callback);
    wx.onTouchMove(callback);
}
function WX_OffTouchMove() {
    OnTouchMoveList.forEach((v) => {
        wx.offTouchMove(v);
    });
}
export default {
    WX_OnTouchMove,
    WX_OffTouchMove,
};
