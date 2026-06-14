#pragma once
#include "pricing.pb.h"

namespace Quant {
    struct GreeksUtils {
        // Compute greeks and populate benchmark sub-message.
        static void ComputeGreeks(const bh_dream::OptionRequestSnapshotProto& req,
                                  bh_dream::OptionGreeksResultSnapshotProto* res);
    };
}
