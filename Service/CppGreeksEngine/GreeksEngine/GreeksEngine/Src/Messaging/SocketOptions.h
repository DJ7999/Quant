#pragma once

namespace Messaging {
    namespace SocketOptions {
        // High-water marks
        inline constexpr int rcv_hwm = 2;
        inline constexpr int snd_hwm = 2;
        inline constexpr int reqrep_rcv_hwm = 16;

        // Timeouts (milliseconds)
        inline constexpr int rcv_timeout_ms = -1; // ventilator pull timeout
        inline constexpr int reqrep_rcv_timeout_ms = 1000; // req/rep receive timeout

        // Buffer sizes (bytes)
        inline constexpr int rcv_buf_bytes = 64 * 1024 * 512;
        inline constexpr int snd_buf_bytes = 64 * 1024 * 512;

        // Linger for REP socket on shutdown (ms)
        inline constexpr int reqrep_linger_ms = 0;
    }
}
